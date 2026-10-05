using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Exceptions;
using COFIDE.WFF.Domain.ValueObjects;
using FluentAssertions;

namespace COFIDE.WFF.Domain.Tests.Tests.Firmas.RestablecerFirma;

public class RestablecerFirmaTest
{
    private static DetalleFirma CrearDetalleFirmado(long id = 1)
    {

        var detalle = DetalleFirma.Crear(id, 10, 20, null, null, null, null, 100);
        detalle.Firmar("contrato", "pdf", "/storage/firmados", "hash-abc", EsInterno: true); detalle.SetearCodigoFirma("COD-123");
        return detalle;
    }

    [Fact]
    public void ObtenerRutasArchivosFirmados_Debe_Concatenar_Ruta_Nombre_Y_Extension()
    {
        var detalle = CrearDetalleFirmado();
        detalle.ObtenerRutasArchivosFirmados()
        .Should().Be("/storage/firmados/contrato.pdf");
    }

    [Fact]
    public void RestablecerFirma_Cuando_Estaba_Firmado_Debe_Volver_A_Pendiente_Y_Limpiar_Metadata()
    {
        var detalle = CrearDetalleFirmado();
        detalle.RestablecerFirma();
        detalle.EstadoDocumento.Should().Be(EstadoDocumento.Pendiente);
        detalle.NombreDocumentoFirmado.Should().BeEmpty();
        detalle.ExtensionDocumentoFirmado.Should().BeEmpty();
        detalle.RutaDocumentoFirmado.Should().BeEmpty();
        detalle.HashDocumentoFirmado.Should().BeEmpty();
        detalle.CodigoFirma.Should().BeEmpty();
        detalle.FechaHoraFirma.Should().BeNull();
        detalle.InternoExterno.Should().BeNull(); 
        detalle.FechaInicioProceso.Should().BeNull(); 
        detalle.FechaFinEsperado.Should().BeNull(); 
        detalle.TieneDocumentoFirmado().Should().BeFalse();
    }


    [Fact]
    public void Firmar_Cuando_Ya_Esta_Firmado_Debe_Lanzar_DomainException()
    {
        var detalle = CrearDetalleFirmado();
        var act = () => detalle.Firmar("x", "pdf", "/r", "h", true);
        act.Should().Throw<DomainException>();
    }

}