using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Exceptions;
using COFIDE.WFF.Domain.Tests.Support;
using COFIDE.WFF.Domain.Tests.Support.Mothers.Solicitudes;
using FluentAssertions;

namespace COFIDE.WFF.Domain.Tests.Features.Solicitudes;

public sealed record DocumentoFirmasCase(
    string Caso,
    List<long> Firmantes,
    List<long> Firmados,
    int FirmasEsperadas,
    int RutasEsperadas)
{
    public override string ToString() => Caso;
}

public class SolicitudDocumentoFirmasTest
{
    public static IEnumerable<object[]> Casos =>
        JsonResource.Cases<DocumentoFirmasCase>("Solicitudes/documento-firmas.json");

    [Theory]
    [MemberData(nameof(Casos))]
    public void ContarFirmas_Debe_Contar_Solo_Los_Detalles_Con_Archivo_Firmado(DocumentoFirmasCase caso)
    {
        var documento = ConDetalles(caso);

        documento.ContarFirmas().Should().Be(caso.FirmasEsperadas);
        documento.ObtenerRutasArchivosFirmados().Should().HaveCount(caso.RutasEsperadas);
    }

    [Theory]
    [MemberData(nameof(Casos))]
    public void RechazarFirmas_Debe_Inactivar_Todos_Los_Detalles_Activos(DocumentoFirmasCase caso)
    {
        var documento = ConDetalles(caso);

        documento.RechazarFirmas();

        documento.DetallesFirma.Should().OnlyContain(d => !d.Estado);
    }

    [Theory]
    [MemberData(nameof(Casos))]
    public void ReactivarFirmas_Debe_Revertir_Un_Rechazo(DocumentoFirmasCase caso)
    {
        var documento = ConDetalles(caso);
        documento.RechazarFirmas();

        documento.ReactivarFirmas();

        documento.DetallesFirma.Should().OnlyContain(d => d.Estado);
    }

    [Fact]
    public void DesactivarDetalleFirmaPorFirmante_Debe_Afectar_Solo_Al_Firmante_Indicado()
    {
        var documento = ConDetalles(new DocumentoFirmasCase("x", [100, 101], [], 0, 0));

        documento.DesactivarDetalleFirmaPorFirmante(100);

        documento.DetallesFirma.Single(d => d.IdFirmanteDocumento == 100).Estado.Should().BeFalse();
        documento.DetallesFirma.Single(d => d.IdFirmanteDocumento == 101).Estado.Should().BeTrue();
    }

    [Fact]
    public void ObtenerDocumentoVigenteParaFirma_Sin_Hash_Debe_Lanzar_DOM_SOL_028()
    {
        var solicitud = SolicitudMother.Borrador();
        var documento = solicitud.AgregarDocumento(1, hash: "");

        var act = () => documento.ObtenerDocumentoVigenteParaFirma(inCodigo: false);

        act.Should().Throw<DomainException>()
           .Which.Error.Code.Should().Be("DOM_SOL_028");
    }

    [Fact]
    public void AgregarDocumento_Debe_Normalizar_La_Extension_A_Minusculas()
    {
        var solicitud = SolicitudMother.Borrador();

        var documento = solicitud.AgregarDocumento(1, extension: "  PDF  ");

        documento.ExtensionOriginal.Should().Be("pdf");
    }

    private static SolicitudDocumento ConDetalles(DocumentoFirmasCase caso)
    {
        var solicitud = SolicitudMother.Borrador();
        var documento = solicitud.AgregarDocumento(1);

        long detalleId = 1;
        foreach (var firmante in caso.Firmantes)
        {
            var detalle = DetalleFirma.Crear(
                detalleId++, idSolicitudFirmanteFlujo: firmante,
                idSolicitudDocumento: documento.Id,
                null, null, null, null, idFirmanteDocumento: firmante);

            if (caso.Firmados.Contains(firmante))
                detalle.Firmar($"doc-{firmante}", "pdf", "/firmados", $"hash-{firmante}", true);

            documento.AgregarDetalleFirma(detalle);
        }

        return documento;
    }
}
