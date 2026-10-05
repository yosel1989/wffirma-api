using COFIDE.WFF.Application.Features.Firmas.DTOs;
using COFIDE.WFF.Application.Features.Firmas.Ports.Out;
using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Services;
using COFIDE.WFF.Application.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using AppEx = COFIDE.WFF.Application.Exceptions;

namespace COFIDE.WFF.Application.Tests.Features.Solicitudes;

public class SolicitudDocumentoTagSyncTest
{
    [Fact]
    public async Task Sincronizar_Tag_Debe_Guardar_El_Pdf_Con_La_Etiqueta()
    {
        var solicitud = Borrador(false);
        solicitud.AgregarDocumento(4, "contrato", "pdf", "originales", "hash",
            null, null, null, null, null, null, null, null, 10);
        var documento = solicitud.Documentos.Single();
        documento.AgregarDocumentoTag(8, 1, 10, 20, 100, 40);
        var archivos = new Mock<IFileStorageService>();
        var inserter = new Mock<IPdfCodigoInserter>();
        var hash = new Mock<IPdfContentHashService>();
        archivos.Setup(a => a.GetFileAsync(Path.Combine("originales", "contrato.pdf"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new MemoryStream([1, 2, 3]));
        archivos.Setup(a => a.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("guardado");
        inserter.Setup(i => i.InsertTag(It.IsAny<byte[]>(), "[F]", 1, 10, 20, "rojo", 12, It.IsAny<CancellationToken>()))
            .Returns(new byte[] { 9 });
        hash.Setup(h => h.ObtenerHashContenido(It.IsAny<byte[]>())).Returns("hash-tag");

        await Servicio(archivos, inserter, hash).SincronizarTagDocumentoAsync(solicitud, CancellationToken.None);

        documento.NombreConTag.Should().Be("contrato_[T]");
        documento.ExtensionConTag.Should().Be("pdf");
        documento.HashConTag.Should().Be("hash-tag");
        documento.RutaConTag.Should().Be("originales");
    }

    [Fact]
    public async Task Sincronizar_Tag_Con_Codigo_Debe_Reescribir_El_Pdf_Codificado()
    {
        var solicitud = Borrador(true);
        solicitud.AgregarDocumento(4, "contrato", "pdf", "originales", "hash",
            "contrato-COD", "pdf", "codigos", "hash-cod", null, null, null, null, 10);
        var documento = solicitud.Documentos.Single();
        documento.AgregarDocumentoTag(8, 1, 10, 20, 100, 40);
        var archivos = new Mock<IFileStorageService>();
        var inserter = new Mock<IPdfCodigoInserter>();
        var hash = new Mock<IPdfContentHashService>();
        archivos.Setup(a => a.GetFileAsync(Path.Combine("codigos", "contrato-COD.pdf"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new MemoryStream([1, 2, 3]));
        archivos.Setup(a => a.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("guardado");
        inserter.Setup(i => i.InsertCodigo(It.IsAny<byte[]>(), solicitud.CodigoSolicitud)).Returns(new byte[] { 4, 4 });
        inserter.Setup(i => i.InsertTag(It.IsAny<byte[]>(), "[F]", 1, 10, 20, "rojo", 12, It.IsAny<CancellationToken>()))
            .Returns(new byte[] { 9 });
        hash.Setup(h => h.ObtenerHashContenido(It.Is<byte[]>(b => b.Length == 2))).Returns("hash-cod-nuevo");
        hash.Setup(h => h.ObtenerHashContenido(It.Is<byte[]>(b => b.Length == 1))).Returns("hash-tag");

        await Servicio(archivos, inserter, hash).SincronizarTagDocumentoAsync(solicitud, CancellationToken.None);

        documento.NombreCodigo.Should().Be("contrato-COD");
        documento.HashCodigo.Should().Be("hash-cod-nuevo");
        documento.NombreConTag.Should().Be("contrato-COD_[T]");
        documento.RutaConTag.Should().Be("codigos");
    }

    [Fact]
    public async Task Crear_Con_Codigo_Y_Tag_Debe_Guardar_Original_Codigo_Y_Etiqueta()
    {
        var archivos = new Mock<IFileStorageService>();
        var inserter = new Mock<IPdfCodigoInserter>();
        var hash = new Mock<IPdfContentHashService>();
        var validador = new Mock<IDocumentoFirmadoValidator>();
        archivos.Setup(a => a.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("guardado");
        inserter.Setup(i => i.InsertCodigo(It.IsAny<byte[]>(), "GTI-SOL")).Returns(new byte[] { 4, 4 });
        inserter.Setup(i => i.InsertTag(It.IsAny<byte[]>(), "[F]", 1, 10, 20, "rojo", 12, It.IsAny<CancellationToken>()))
            .Returns(new byte[] { 9 });
        hash.Setup(h => h.ObtenerHashContenido(It.Is<byte[]>(b => b.Length == 3))).Returns("hash-1");
        hash.Setup(h => h.ObtenerHashContenido(It.Is<byte[]>(b => b.Length == 2))).Returns("hash-cod");
        hash.Setup(h => h.ObtenerHashContenido(It.Is<byte[]>(b => b.Length == 1))).Returns("hash-tag");
        validador.Setup(v => v.TieneFirmaAsync(It.IsAny<Stream>())).ReturnsAsync(false);

        var resultado = await Servicio(archivos, inserter, hash, validador).CrearDocumentoInput(
            [Archivo("contrato.pdf", [1, 2, 3])],
            "GTI-SOL",
            true,
            """[{"nombreDocumento":"contrato.pdf","coordenadas":[{"numeroPagina":1,"x":10,"y":20}]}]""",
            CancellationToken.None);

        var documento = resultado.Single();
        documento.NombreOriginal.Should().Be("contrato");
        documento.NombreCodigo.Should().Be("contrato-COD");
        documento.HashCodigo.Should().Be("hash-cod");
        documento.NombreConTag.Should().Be("contrato-COD_[T]");
        documento.HashConTag.Should().Be("hash-tag");
        documento.PesoDocumento.Should().Be(3);
    }

    [Fact]
    public async Task Crear_Con_Tag_Invalido_Debe_Lanzar_APP_SOL_011()
    {
        var validador = new Mock<IDocumentoFirmadoValidator>();
        validador.Setup(v => v.TieneFirmaAsync(It.IsAny<Stream>())).ReturnsAsync(false);
        var archivos = new Mock<IFileStorageService>();
        archivos.Setup(a => a.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("guardado");
        var hash = new Mock<IPdfContentHashService>();
        hash.Setup(h => h.ObtenerHashContenido(It.IsAny<byte[]>())).Returns("hash-1");

        var act = () => Servicio(archivos, hash: hash, validador: validador).CrearDocumentoInput(
            [Archivo("contrato.pdf", [1, 2, 3])], "GTI-SOL", false, "no-es-json", CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be("APP_SOL_011");
    }

    private static SolicitudDocumentoService Servicio(
        Mock<IFileStorageService>? archivos = null,
        Mock<IPdfCodigoInserter>? inserter = null,
        Mock<IPdfContentHashService>? hash = null,
        Mock<IDocumentoFirmadoValidator>? validador = null)
    {
        var parametros = new Mock<IParametroRepository>();
        parametros.Setup(p => p.ConfiguracionFirmaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConfiguracionFirmaDto { NombreTag = "[F]", ColorEtiqueta = "rojo", FirmaMargenAlto = 12 });
        return new SolicitudDocumentoService(
            (archivos ?? new Mock<IFileStorageService>()).Object,
            (inserter ?? new Mock<IPdfCodigoInserter>()).Object,
            (hash ?? new Mock<IPdfContentHashService>()).Object,
            (validador ?? new Mock<IDocumentoFirmadoValidator>()).Object,
            parametros.Object);
    }

    private static IFormFile Archivo(string nombre, byte[] contenido)
    {
        var archivo = new Mock<IFormFile>();
        archivo.Setup(a => a.FileName).Returns(nombre);
        archivo.Setup(a => a.OpenReadStream()).Returns(() => new MemoryStream(contenido));
        return archivo.Object;
    }

    private static Solicitud Borrador(bool inCodigo) =>
        Solicitud.CrearBorrador(8, "GTI-SOL-00000008-2026", 1, 2, 10, 3, 10, 5, 2,
            "Contrato", null, false, inCodigo, null, 0);
}
