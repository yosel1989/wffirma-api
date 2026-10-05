using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Events;
using COFIDE.WFF.Domain.Exceptions;
using COFIDE.WFF.Domain.Tests.Support;
using COFIDE.WFF.Domain.Tests.Support.Mothers.Solicitudes;
using COFIDE.WFF.Domain.ValueObjects;
using FluentAssertions;

namespace COFIDE.WFF.Domain.Tests.Features.Solicitudes;

public sealed record UrgenciaCase(
    string Caso,
    bool EsUrgente,
    int? DiasFechaLimite,
    bool EsUrgenteEsperado,
    bool DebeTenerFechaLimite,
    string? CodigoError)
{
    public override string ToString() => Caso;
}

public class SolicitudUrgenciaTest
{
    public static IEnumerable<object[]> Casos =>
        JsonResource.Cases<UrgenciaCase>("Solicitudes/urgencia.json");

    [Theory]
    [MemberData(nameof(Casos))]
    public void CrearBorrador_Debe_Aplicar_Las_Reglas_De_Urgencia(UrgenciaCase caso)
    {
        DateTime? fechaLimite = caso.DiasFechaLimite is { } dias
            ? DateTime.Now.AddDays(dias)
            : null;

        var act = () => SolicitudMother.Borrador(esUrgente: caso.EsUrgente, fechaLimite: fechaLimite);

        if (caso.CodigoError is not null)
        {
            act.Should().Throw<DomainException>()
               .Which.Error.Code.Should().Be(caso.CodigoError);
            return;
        }

        var solicitud = act();

        solicitud.EsUrgente.Should().Be(caso.EsUrgenteEsperado);
        (solicitud.FechaLimite is not null).Should().Be(caso.DebeTenerFechaLimite);
    }

    [Theory]
    [MemberData(nameof(Casos))]
    public void Finalizar_Debe_Reaplicar_Las_Mismas_Reglas_De_Urgencia(UrgenciaCase caso)
    {
        DateTime? fechaLimite = caso.DiasFechaLimite is { } dias
            ? DateTime.Now.AddDays(dias)
            : null;

        var solicitud = SolicitudMother.Borrador();

        var act = () => solicitud.Finalizar(
            "Contrato", null, caso.EsUrgente, false, fechaLimite, 100, "ana@cofide.com.pe");

        if (caso.CodigoError is not null)
        {
            act.Should().Throw<DomainException>()
               .Which.Error.Code.Should().Be(caso.CodigoError);

            // La transición no debe aplicarse parcialmente.
            solicitud.EstadoSolicitud.Should().Be(EstadoSolicitud.Borrador);
            solicitud.DomainEvents.Should().BeEmpty();
            return;
        }

        act.Should().NotThrow();
        solicitud.EstadoSolicitud.Should().Be(EstadoSolicitud.EnCurso);
    }

    [Fact]
    public void CrearBorrador_Debe_Nacer_En_Borrador_Activa_Y_Sin_Eventos()
    {
        var solicitud = SolicitudMother.Borrador();

        solicitud.EstadoSolicitud.Should().Be(EstadoSolicitud.Borrador);
        solicitud.IdEstadoSolicitud.Should().Be(1);
        solicitud.Estado.Should().BeTrue();
        solicitud.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Finalizar_Debe_Emitir_Creada_Y_PorAprobarFirmantes()
    {
        var solicitud = SolicitudMother.Borrador();

        solicitud.Finalizar("Contrato", null, false, false, null, 2500, "ana@cofide.com.pe");

        solicitud.DomainEvents.Should().HaveCount(2);
        solicitud.DomainEvents.Should().ContainSingle(e => e is SolicitudCreadaDomainEvent);
        solicitud.DomainEvents.Should().ContainSingle(e => e is SolicitudPorAprobarFirmantesDomainEvent);
    }

    [Fact]
    public void Finalizar_Dos_Veces_Debe_Lanzar_DOM_SOL_001()
    {
        var solicitud = SolicitudMother.EnCurso();

        var act = () => solicitud.Finalizar("Contrato", null, false, false, null, 0, null);

        act.Should().Throw<DomainException>()
           .Which.Error.Code.Should().Be("DOM_SOL_001");
    }

    [Fact]
    public void Finalizar_Debe_Sobrescribir_El_Peso_Calculado_Con_El_Del_Request()
    {
        var solicitud = SolicitudMother.Borrador();
        solicitud.AgregarDocumento(1, peso: 4000);
        solicitud.CalcularPesoTotalDocumentos();
        solicitud.PesoTotalDocu.Should().Be(4000);

        solicitud.Finalizar("Contrato", null, false, false, null, null, null);

        // Comportamiento actual: el peso del request reemplaza el calculado.
        solicitud.PesoTotalDocu.Should().BeNull();
    }
}
