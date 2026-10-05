using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Exceptions;
using COFIDE.WFF.Domain.Tests.Support;
using COFIDE.WFF.Domain.Tests.Support.Mothers.Solicitudes;
using FluentAssertions;

namespace COFIDE.WFF.Domain.Tests.Features.Solicitudes;

public sealed record SincronizacionCase(
    string Caso,
    List<string> Existentes,
    List<string> Request,
    int NuevosEsperados,
    List<string> ActivosEsperados)
{
    public override string ToString() => Caso;
}

public class SolicitudSincronizacionTest
{
    public static IEnumerable<object[]> Casos =>
        JsonResource.Cases<SincronizacionCase>("Solicitudes/sincronizacion-codigos.json");

    [Theory]
    [MemberData(nameof(Casos))]
    public void ContarNuevosCodigosReferencia_Debe_Contar_Solo_Los_Que_No_Existen(SincronizacionCase caso)
    {
        var solicitud = ConCodigos(caso.Existentes);

        // Este conteo es el que Application usa para reservar bloques de IDs.
        solicitud.ContarNuevosCodigosReferencia(caso.Request)
            .Should().Be(caso.NuevosEsperados);
    }

    [Theory]
    [MemberData(nameof(Casos))]
    public void SincronizarCodigosReferencia_Debe_Dejar_Activos_Solo_Los_Del_Request(SincronizacionCase caso)
    {
        var solicitud = ConCodigos(caso.Existentes);
        var ids = SolicitudMother.Ids(500);

        solicitud.SincronizarCodigosReferencia(caso.Request, ids);

        solicitud.CodigosReferencia
            .Where(c => c.Estado)
            .Select(c => c.CodigoReferencia.Valor)
            .Should().BeEquivalentTo(caso.ActivosEsperados);
    }

    [Theory]
    [MemberData(nameof(Casos))]
    public void SincronizarCodigosReferencia_Debe_Consumir_Un_Id_Por_Codigo_Nuevo(SincronizacionCase caso)
    {
        var solicitud = ConCodigos(caso.Existentes);

        var consumidos = 0;
        Func<long> ids = () => { consumidos++; return 900 + consumidos; };

        solicitud.SincronizarCodigosReferencia(caso.Request, ids);

        // Si esto falla, Application reservaría bloques de IDs de tamaño incorrecto.
        consumidos.Should().Be(caso.NuevosEsperados);
    }

    [Fact]
    public void SincronizarCorreos_Debe_Conservar_El_Correo_Repetido_Con_Otra_Capitalizacion()
    {
        var solicitud = SolicitudMother.Borrador();
        solicitud.AgregarCorreo(1, "ana@cofide.com.pe");

        solicitud.SincronizarCorreos(
            ["ANA@COFIDE.COM.PE", "luis@cofide.com.pe"],
            SolicitudMother.Ids(500));

        solicitud.Correos.Where(c => c.Estado)
            .Select(c => c.Correo.Value)
            .Should().BeEquivalentTo("ana@cofide.com.pe", "luis@cofide.com.pe");
    }

    [Fact]
    public void CalcularPesoTotalDocumentos_Debe_Excluir_Los_Documentos_Inactivos()
    {
        var solicitud = SolicitudMother.Borrador();
        solicitud.AgregarDocumento(1, peso: 1000);
        var segundo = solicitud.AgregarDocumento(2, peso: 500);

        segundo.Inactivar();
        solicitud.CalcularPesoTotalDocumentos();

        solicitud.PesoTotalDocu.Should().Be(1000);
    }

    [Fact]
    public void AgregarDocumento_Al_Llegar_A_Treinta_Debe_Lanzar_DOM_SOL_010()
    {
        var solicitud = SolicitudMother.Borrador();
        for (long i = 1; i <= 30; i++)
            solicitud.AgregarDocumento(i, peso: 1);

        var act = () => solicitud.AgregarDocumento(31, peso: 1);

        act.Should().Throw<DomainException>()
           .Which.Error.Code.Should().Be("DOM_SOL_010");
    }

    private static Solicitud ConCodigos(
        List<string> codigos)
    {
        var solicitud = SolicitudMother.Borrador();
        long id = 1;

        foreach (var codigo in codigos)
            solicitud.AgregarCodReferencia(id++, codigo);

        return solicitud;
    }
}