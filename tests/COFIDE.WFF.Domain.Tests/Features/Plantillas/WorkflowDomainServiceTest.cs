using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.DomainServices;
using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Domain.Tests.Common.TestSupport;
using COFIDE.WFF.Domain.Tests.Resources.Features.Plantillas;
using COFIDE.WFF.Domain.Tests.Resources.Features.Solicitudes;
using FluentAssertions;
using Vo = COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Vo;

namespace COFIDE.WFF.Domain.Tests.Features.Plantillas;

public sealed record EtapaWorkflowCase(long Orden, long IdTipoAprobador, List<long> Usuarios);

public sealed record WorkflowCase(
    string Caso,
    List<EtapaWorkflowCase> Etapas,
    long IdFirmante,
    long IdVisador,
    int FirmantesFlujoEsperados,
    bool EsFirmanteEsperado,
    bool EsVisadorEsperado)
{
    public override string ToString() => Caso;
}

public class WorkflowDomainServiceTest
{
    private readonly WorkflowDomainService _sut = new();

    public static IEnumerable<object[]> Casos =>
        JsonResource.Cases<WorkflowCase>("Plantillas/workflow-tipo-aprobador.json");

    [Theory]
    [MemberData(nameof(Casos))]
    public void AgregarFirmantesFlujoDesdeEtapas_Debe_Crear_Un_Flujo_Por_Firmante(WorkflowCase caso)
    {
        var (solicitud, plantilla) = Montar(caso);

        _sut.AgregarFirmantesFlujoDesdeEtapas(solicitud, plantilla, SolicitudMother.Ids(500));

        var flujo = solicitud.SolicitudPlantilla.FirmantesFlujo;

        flujo.Should().HaveCount(caso.FirmantesFlujoEsperados);
        flujo.Should().OnlyContain(f => f.Estado && !f.RevAprobado);
        flujo.Select(f => f.Id).Should().OnlyHaveUniqueItems();
        flujo.Select(f => f.IdUsuarioAsignado)
             .Should().BeEquivalentTo(caso.Etapas.SelectMany(e => e.Usuarios));
    }

    [Theory]
    [MemberData(nameof(Casos))]
    public void ConstruirFirmanteFlujoEtapa_Debe_Propagar_Orden_Y_Tipo_De_Aprobador(WorkflowCase caso)
    {
        var (solicitud, plantilla) = Montar(caso);
        _sut.AgregarFirmantesFlujoDesdeEtapas(solicitud, plantilla, SolicitudMother.Ids(500));

        var dict = DiccionarioTipoAprobador(caso);

        _sut.ConstruirFirmanteFlujoEtapa(solicitud, plantilla, dict);

        var flujo = solicitud.SolicitudPlantilla.FirmantesFlujo;

        flujo.Should().OnlyContain(f => f.EsFirmante == caso.EsFirmanteEsperado);
        flujo.Should().OnlyContain(f => f.EsVisador == caso.EsVisadorEsperado);
        flujo.Select(f => f.OrdenEtapa)
             .Should().BeEquivalentTo(
                 caso.Etapas.SelectMany(e => e.Usuarios.Select(_ => (long?)e.Orden)));
    }

    [Fact]
    public void ConstruirFirmanteFlujoEtapa_Con_Diccionario_Incompleto_Debe_Lanzar_DOM_PLT_017()
    {
        var caso = new WorkflowCase("local",
            [new EtapaWorkflowCase(1, 10, [100])], 10, 11, 1, true, false);

        var (solicitud, plantilla) = Montar(caso);
        _sut.AgregarFirmantesFlujoDesdeEtapas(solicitud, plantilla, SolicitudMother.Ids(500));

        var act = () => _sut.ConstruirFirmanteFlujoEtapa(
            solicitud, plantilla, new Dictionary<long, ParametroDetalle>());

        act.Should().Throw<Exceptions.DomainException>()
           .Which.Error.Code.Should().Be("DOM_PLT_017");
    }

    [Fact]
    public void FlujoEstaCompleto_Debe_Ser_Verdadero_Solo_Cuando_Todos_Aprobaron()
    {
        var caso = new WorkflowCase("local",
            [new EtapaWorkflowCase(1, 10, [100, 101])], 10, 11, 2, true, false);

        var (solicitud, plantilla) = Montar(caso);
        _sut.AgregarFirmantesFlujoDesdeEtapas(solicitud, plantilla, SolicitudMother.Ids(500));

        var sp = solicitud.SolicitudPlantilla;
        sp.FlujoEstaCompleto().Should().BeFalse();

        sp.ObtenerFirmanteActivo(100).Aprobar();
        sp.FlujoEstaCompleto().Should().BeFalse();

        sp.ObtenerFirmanteActivo(101).Aprobar();
        sp.FlujoEstaCompleto().Should().BeTrue();
    }

    [Fact]
    public void ObtenerFirmanteActivo_De_Un_Usuario_Ya_Aprobado_Debe_Lanzar_DOM_SOL_015()
    {
        var caso = new WorkflowCase("local",
            [new EtapaWorkflowCase(1, 10, [100])], 10, 11, 1, true, false);

        var (solicitud, plantilla) = Montar(caso);
        _sut.AgregarFirmantesFlujoDesdeEtapas(solicitud, plantilla, SolicitudMother.Ids(500));

        var sp = solicitud.SolicitudPlantilla;
        sp.ObtenerFirmanteActivo(100).Aprobar();

        var act = () => sp.ObtenerFirmanteActivo(100);

        act.Should().Throw<Exceptions.DomainException>()
           .Which.Error.Code.Should().Be("DOM_SOL_015");
    }

    [Fact]
    public void ResetearFirmantes_Debe_Devolver_El_Flujo_Al_Estado_Inicial()
    {
        var caso = new WorkflowCase("local",
            [new EtapaWorkflowCase(1, 10, [100, 101])], 10, 11, 2, true, false);

        var (solicitud, plantilla) = Montar(caso);
        _sut.AgregarFirmantesFlujoDesdeEtapas(solicitud, plantilla, SolicitudMother.Ids(500));

        var sp = solicitud.SolicitudPlantilla;
        sp.ObtenerFirmanteActivo(100).Aprobar();

        sp.ResetearFirmantes();

        sp.FirmantesFlujo.Should().OnlyContain(f => !f.RevAprobado && f.FechaRevision == null);
    }

    private static (Solicitud, Plantilla) Montar(WorkflowCase caso)
    {
        var plantilla = PlantillaMother.Vacia();
        long etapaId = 1, firmanteId = 1;

        foreach (var etapaCaso in caso.Etapas)
        {
            var tipoAprobador = etapaCaso.IdTipoAprobador == caso.IdVisador
                ? Vo.TipoAprobador.Visador
                : Vo.TipoAprobador.Firmante;

            var etapa = plantilla.AgregarEtapas(
                etapaId++, etapaCaso.Orden, $"Etapa {etapaCaso.Orden}",
                Vo.TipoEtapa.Secuencial, idTipoEtapa: 1,
                tipoAprobador, etapaCaso.IdTipoAprobador);

            foreach (var usuario in etapaCaso.Usuarios)
                etapa.AgregarFirmante(firmanteId++, usuario, "Jefe", "GTI", "GTI", 10);
        }

        var solicitud = SolicitudMother.Borrador();
        solicitud.AsignarPlantilla(SolicitudPlantilla.Crear(1, solicitud, plantilla));

        return (solicitud, plantilla);
    }

    private static Dictionary<long, ParametroDetalle> DiccionarioTipoAprobador(WorkflowCase caso) =>
        new()
        {
            [caso.IdFirmante] = new(caso.IdFirmante, "TI_APROBADOR_FIRMANTE", "F"),
            [caso.IdVisador] = new(caso.IdVisador, "TI_APROBADOR_VISADOR", "V")
        };
}