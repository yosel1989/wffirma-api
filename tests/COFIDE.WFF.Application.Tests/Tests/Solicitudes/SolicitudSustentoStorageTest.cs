using COFIDE.WFF.Application.Features.Solicitudes.Services;
using COFIDE.WFF.Application.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using FluentAssertions;
using Moq;

namespace COFIDE.WFF.Application.Tests.Features.Solicitudes;

public class SolicitudSustentoStorageTest
{
    [Fact]
    public async Task Sin_Ids_Conservados_Debe_Responder_Vacio()
    {
        var solicitud = Borrador();
        solicitud.AgregarSustento(6, "anexo", "pdf", "sustentos", 10);
        var archivos = new Mock<IFileStorageService>();

        var resultado = await new SolicitudSustentoService(archivos.Object)
            .CrearDocumentoSustentoInputDesdeStorage(solicitud.Sustentos.ToList(), [99], "GTI-SOL", CancellationToken.None);

        resultado.Should().BeEmpty();
        archivos.Verify(a => a.GetFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Debe_Reprocesar_El_Sustento_Conservado()
    {
        var solicitud = Borrador();
        solicitud.AgregarSustento(6, "anexo", "pdf", "sustentos", 10);
        var archivos = new Mock<IFileStorageService>();
        archivos.Setup(a => a.GetFileAsync(Path.Combine("sustentos", "anexo.pdf"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new MemoryStream([1, 2, 3]));
        archivos.Setup(a => a.SaveFileAsync(It.IsAny<Stream>(), "anexo.pdf", It.IsAny<string>(), "anexo.pdf", It.IsAny<CancellationToken>()))
            .ReturnsAsync("guardado");

        var resultado = await new SolicitudSustentoService(archivos.Object)
            .CrearDocumentoSustentoInputDesdeStorage(solicitud.Sustentos.ToList(), [6], "GTI-SOL", CancellationToken.None);

        var sustento = resultado.Single();
        sustento.NombreSustento.Should().Be("anexo");
        sustento.ExtensionSustento.Should().Be("pdf");
        sustento.RutaSustento.Should().Be(Path.Combine("GTI-SOL", "SUSTENTOS"));
        sustento.PesoSustento.Should().Be(3);
    }

    private static Solicitud Borrador() =>
        Solicitud.CrearBorrador(8, "GTI-SOL-00000008-2026", 1, 2, 10, 3, 10, 5, 2,
            "Contrato", null, false, false, null, 0);
}
