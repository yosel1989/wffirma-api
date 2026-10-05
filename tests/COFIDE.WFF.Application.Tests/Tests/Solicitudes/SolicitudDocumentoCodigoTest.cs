using COFIDE.WFF.Application.Features.Firmas.Ports.Out;
using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Services;
using COFIDE.WFF.Application.Ports.Out;
using COFIDE.WFF.Application.Tests.Support;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using FluentAssertions;
using Moq;

namespace COFIDE.WFF.Application.Tests.Solicitudes;

public class SolicitudDocumentoCodigoTest
{
    [Fact]
    public async Task DesdeStorage_Debe_Reprocesar_El_Documento_Conservado()
    {
        var solicitud = Borrador(false);
        solicitud.AgregarDocumento(4, "contrato", "pdf", "originales", "hash",
            null, null, null, null, null, null, null, null, 10);
        var archivos = ArchivosDePrueba.Almacenamiento();
        var hash = new Mock<IPdfContentHashService>();
        var validador = new Mock<IDocumentoFirmadoValidator>();
        archivos.Setup(a => a.GetFileAsync(Path.Combine("originales", "contrato.pdf"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new MemoryStream([1, 2, 3]));
        archivos.Setup(a => a.SaveFileAsync(It.IsAny<Stream>(), "contrato.pdf", It.IsAny<string>(), "contrato.pdf", It.IsAny<CancellationToken>()))
            .ReturnsAsync("guardado");
        hash.Setup(h => h.ObtenerHashContenido(It.IsAny<byte[]>())).Returns("hash-1");
        validador.Setup(v => v.TieneFirmaAsync(It.IsAny<Stream>())).ReturnsAsync(true);

        var resultado = await Servicio(archivos, hash, validador).CrearDocumentoInputDesdeStorage(
            solicitud.Documentos.ToList(), [4], "GTI-SOL", false, CancellationToken.None);

        var documento = resultado.Single();
        documento.NombreOriginal.Should().Be("contrato");
        documento.NombreCodigo.Should().Be("contrato");
        documento.HashOriginal.Should().Be("hash-1");
        documento.PesoDocumento.Should().Be(3);
    }

    [Fact]
    public async Task Activar_Codigo_Debe_Guardar_La_Copia_Codificada()
    {
        var solicitud = Borrador(false);
        solicitud.AgregarDocumento(4, "contrato", "pdf", "originales", "hash-original",
            null, null, null, null, null, null, null, null, 10);
        var documento = solicitud.Documentos.Single();
        var archivos = ArchivosDePrueba.Almacenamiento();
        var hash = new Mock<IPdfContentHashService>();
        var inserter = new Mock<IPdfCodigoInserter>();
        var validador = new Mock<IDocumentoFirmadoValidator>();
        archivos.Setup(a => a.GetFileAsync(Path.Combine("originales", "contrato.pdf"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new MemoryStream([1, 2, 3]));
        archivos.Setup(a => a.SaveFileAsync(It.IsAny<Stream>(), "contrato-COD.pdf", It.IsAny<string>(), "contrato-COD.pdf", It.IsAny<CancellationToken>()))
            .ReturnsAsync("guardado");
        inserter.Setup(i => i.InsertCodigo(It.IsAny<byte[]>(), solicitud.CodigoSolicitud)).Returns([9, 9]);
        hash.Setup(h => h.ObtenerHashContenido(It.IsAny<byte[]>())).Returns("hash-cod");
        validador.Setup(v => v.TieneFirmaAsync(It.IsAny<Stream>())).ReturnsAsync(false);

        await Servicio(archivos, hash, validador, inserter).SincronizarCambioCodigoAsync(solicitud, true, CancellationToken.None);

        documento.NombreCodigo.Should().Be("contrato-COD");
        documento.ExtensionCodigo.Should().Be("pdf");
        documento.HashCodigo.Should().Be("hash-cod");
        documento.RutaCodigo.Should().Be(Path.Combine(solicitud.CodigoSolicitud, "DOCUMENTOS"));
    }

    [Fact]
    public async Task Activar_Codigo_Con_Firma_Debe_Reutilizar_El_Original()
    {
        var solicitud = Borrador(false);
        solicitud.AgregarDocumento(4, "contrato", "pdf", "originales", "hash-original",
            null, null, null, null, null, null, null, null, 10);
        var documento = solicitud.Documentos.Single();
        var archivos = ArchivosDePrueba.Almacenamiento();
        var validador = new Mock<IDocumentoFirmadoValidator>();
        archivos.Setup(a => a.GetFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new MemoryStream([1]));
        validador.Setup(v => v.TieneFirmaAsync(It.IsAny<Stream>())).ReturnsAsync(true);

        await Servicio(archivos, validador: validador).SincronizarCambioCodigoAsync(solicitud, true, CancellationToken.None);

        documento.NombreCodigo.Should().Be("contrato");
        documento.HashCodigo.Should().Be("hash-original");
        documento.RutaCodigo.Should().Be("originales");
        archivos.Verify(a => a.SaveFileAsync(
            It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static SolicitudDocumentoService Servicio(
        Mock<IFileStorageService>? archivos = null,
        Mock<IPdfContentHashService>? hash = null,
        Mock<IDocumentoFirmadoValidator>? validador = null,
        Mock<IPdfCodigoInserter>? inserter = null) =>
        new(
            (archivos ?? new Mock<IFileStorageService>()).Object,
            (inserter ?? new Mock<IPdfCodigoInserter>()).Object,
            (hash ?? new Mock<IPdfContentHashService>()).Object,
            (validador ?? new Mock<IDocumentoFirmadoValidator>()).Object,
            new Mock<IParametroRepository>().Object);

    private static Solicitud Borrador(bool inCodigo) =>
        Solicitud.CrearBorrador(8, "GTI-SOL-00000008-2026", 1, 2, 10, 3, 10, 5, 2,
            "Contrato", null, false, inCodigo, null, 0);
}
