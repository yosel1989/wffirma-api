using COFIDE.WFF.Application.Features.Firmas.DTOs;
using COFIDE.WFF.Application.Features.Firmas.Ports.Out;
using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.DTOs;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Services;
using COFIDE.WFF.Application.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Inputs;
using FluentAssertions;
using Moq;

namespace COFIDE.WFF.Application.Tests.Features.Solicitudes;

public class SolicitudDocumentoSincronizacionTest
{
    [Fact]
    public async Task Cambio_De_Codigo_Igual_No_Debe_Tocar_Archivos()
    {
        var archivos = new Mock<IFileStorageService>();

        await Servicio(archivos).SincronizarCambioCodigoAsync(Borrador(false), false, CancellationToken.None);

        archivos.Verify(a => a.DeleteFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Cambio_De_Codigo_Sin_Documentos_Activos_No_Debe_Generar_Archivo()
    {
        var solicitud = Borrador(false);
        solicitud.AgregarDocumento(4, "contrato", "pdf", "originales", "hash",
            null, null, null, null, null, null, null, null, 10);
        solicitud.Documentos.Single().Estado = false;
        var archivos = new Mock<IFileStorageService>();

        await Servicio(archivos).SincronizarCambioCodigoAsync(solicitud, true, CancellationToken.None);

        archivos.Verify(a => a.GetFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Quitar_Codigo_Debe_Borrar_El_Archivo_Codificado()
    {
        var solicitud = Borrador(true);
        solicitud.AgregarDocumento(4, "contrato", "pdf", "originales", "hash",
            "contrato-COD", "pdf", "codigos", "hash-cod", null, null, null, null, 10);
        var documento = solicitud.Documentos.Single();
        var archivos = new Mock<IFileStorageService>();
        archivos.Setup(a => a.DeleteFileAsync(Path.Combine("codigos", "contrato-COD.pdf"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Servicio(archivos).SincronizarCambioCodigoAsync(solicitud, false, CancellationToken.None);

        documento.NombreCodigo.Should().BeNull();
        documento.RutaCodigo.Should().BeNull();
        archivos.Verify(a => a.DeleteFileAsync(Path.Combine("codigos", "contrato-COD.pdf"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Desde_Storage_Sin_Ids_Conservados_Debe_Responder_Vacio()
    {
        var solicitud = Borrador(false);
        solicitud.AgregarDocumento(4, "contrato", "pdf", "originales", "hash",
            null, null, null, null, null, null, null, null, 10);
        var archivos = new Mock<IFileStorageService>();

        var resultado = await Servicio(archivos).CrearDocumentoInputDesdeStorage(
            solicitud.Documentos.ToList(), [99], "GTI-SOL", false, CancellationToken.None);

        resultado.Should().BeEmpty();
        archivos.Verify(a => a.GetFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Tag_Sin_Documentos_No_Debe_Leer_Archivos()
    {
        var archivos = new Mock<IFileStorageService>();
        var parametros = new Mock<IParametroRepository>();
        parametros.Setup(p => p.ConfiguracionFirmaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConfiguracionFirmaDto());

        await Servicio(archivos, parametros).SincronizarTagDocumentoAsync(Borrador(false), CancellationToken.None);

        archivos.Verify(a => a.GetFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AgregarTag_Sin_Coordenadas_Debe_Devolver_El_Mismo_Documento()
    {
        var parametros = new Mock<IParametroRepository>();
        var request = new DocumentoProcesadoInput { NombreOriginal = "contrato", ExtensionOriginal = "pdf" };

        var resultado = await Servicio(parametros: parametros).AgregarTag(
            false, "docs", request, [1], [], CancellationToken.None);

        resultado.Should().BeSameAs(request);
        resultado.NombreConTag.Should().BeNull();
        parametros.Verify(p => p.ConfiguracionFirmaAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AgregarTag_Debe_Guardar_El_Pdf_Con_Sufijo()
    {
        var archivos = new Mock<IFileStorageService>();
        var parametros = new Mock<IParametroRepository>();
        var inserter = new Mock<IPdfCodigoInserter>();
        var hash = new Mock<IPdfContentHashService>();
        var request = new DocumentoProcesadoInput
        {
            NombreOriginal = "contrato",
            NombreCodigo = "contrato-COD",
            ExtensionOriginal = "pdf"
        };
        parametros.Setup(p => p.ConfiguracionFirmaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConfiguracionFirmaDto { NombreTag = "[F]", ColorEtiqueta = "rojo", FirmaMargenAlto = 12 });
        inserter.Setup(i => i.InsertTag(It.IsAny<byte[]>(), "[F]", 1, 10, 20, "rojo", 12, It.IsAny<CancellationToken>()))
            .Returns([9]);
        hash.Setup(h => h.ObtenerHashContenido(It.IsAny<byte[]>())).Returns("hash-tag");
        archivos.Setup(a => a.SaveFileAsync(It.IsAny<Stream>(), "contrato_[T].pdf", "docs", "contrato_[T].pdf", It.IsAny<CancellationToken>()))
            .ReturnsAsync("guardado");

        var resultado = await new SolicitudDocumentoService(
                archivos.Object, inserter.Object, hash.Object, new Mock<IDocumentoFirmadoValidator>().Object, parametros.Object)
            .AgregarTag(false, "docs", request, [1],
                [new DocumentoTagRequestDto(null, "contrato.pdf", [new Coordenada { NumeroPagina = 1, X = 10, Y = 20 }])],
                CancellationToken.None);

        resultado.NombreConTag.Should().Be("contrato_[T]");
        resultado.ExtensionConTag.Should().Be("pdf");
        resultado.HashConTag.Should().Be("hash-tag");
        resultado.RutaConTag.Should().Be("docs");
    }

    private static SolicitudDocumentoService Servicio(
        Mock<IFileStorageService>? archivos = null,
        Mock<IParametroRepository>? parametros = null) =>
        new(
            (archivos ?? new Mock<IFileStorageService>()).Object,
            new Mock<IPdfCodigoInserter>().Object,
            new Mock<IPdfContentHashService>().Object,
            new Mock<IDocumentoFirmadoValidator>().Object,
            (parametros ?? new Mock<IParametroRepository>()).Object);

    private static Solicitud Borrador(bool inCodigo) =>
        Solicitud.CrearBorrador(8, "GTI-SOL-00000008-2026", 1, 2, 10, 3, 10, 5, 2,
            "Contrato", null, false, inCodigo, null, 0);
}
