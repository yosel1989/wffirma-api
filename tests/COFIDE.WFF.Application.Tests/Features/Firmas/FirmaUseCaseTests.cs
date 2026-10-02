using AutoMapper;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Firmas.Commands.FirmaExterna;
using COFIDE.WFF.Application.Features.Firmas.Commands.FirmaInternaPdf;
using COFIDE.WFF.Application.Features.Firmas.Commands.FirmaInternaZip;
using COFIDE.WFF.Application.Features.Firmas.Commands.RestablecerDocumentoFirmado;
using COFIDE.WFF.Application.Features.Firmas.Ports.Out;
using COFIDE.WFF.Application.Features.Firmas.Services;
using COFIDE.WFF.Application.Features.Firmas.UseCases;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using AppEx = COFIDE.WFF.Application.Exceptions;

namespace COFIDE.WFF.Application.Tests.Features.Firmas;

public class FirmaUseCaseTests
{
    private readonly Mock<IMapper> _mapper = new();

    [Fact]
    public async Task Restablecer_Sin_Detalles_Firmados_Debe_Responder_Vacio()
    {
        var detalles = new Mock<IDetalleFirmaRepository>();
        var archivos = new Mock<IFileStorageService>();
        detalles.Setup(d => d.ObtenerDocumentosEnEstadoFirmadoAsync(It.IsAny<List<long>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var resultado = await new RestablecerDocumentoFirmadoUseCase(detalles.Object, archivos.Object)
            .RestablecerFirma(new RestablecerDocumentoFirmadoCommand([1]), CancellationToken.None);

        resultado.Should().BeEmpty();
        archivos.Verify(a => a.DeleteFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Restablecer_Debe_Borrar_El_Archivo_Y_Dejar_El_Detalle_Pendiente()
    {
        var detalle = DetalleFirma.Crear(1, 2, 4, null, null, null, null, 100);
        detalle.Firmar("contrato [F]", "pdf", "firmados", "hash", true);
        var detalles = new Mock<IDetalleFirmaRepository>();
        var archivos = new Mock<IFileStorageService>();
        detalles.Setup(d => d.ObtenerDocumentosEnEstadoFirmadoAsync(It.IsAny<List<long>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([detalle]);
        detalles.Setup(d => d.UpdateRangeAsync(It.IsAny<IEnumerable<DetalleFirma>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        archivos.Setup(a => a.DeleteFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var resultado = await new RestablecerDocumentoFirmadoUseCase(detalles.Object, archivos.Object)
            .RestablecerFirma(new RestablecerDocumentoFirmadoCommand([1]), CancellationToken.None);

        resultado.Should().BeEmpty();
        detalle.EstadoDocumento.Should().Be(EstadoDocumento.Pendiente);
        archivos.Verify(a => a.DeleteFileAsync("firmados/contrato [F].pdf", It.IsAny<CancellationToken>()), Times.Once);
        detalles.Verify(d => d.UpdateRangeAsync(It.IsAny<IEnumerable<DetalleFirma>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task FirmaPdf_Sin_Documento_Debe_Lanzar_APP_SOL_015()
    {
        var lecturas = new Mock<ISolicitudReadPort>();
        lecturas.Setup(l => l.ObtenerDocumentoParaFirmaInternaMasivaRead(8, 4, It.IsAny<CancellationToken>(), true))
            .ReturnsAsync((SolicitudDocumento?)null);

        var act = () => Pdf(lecturas).Firmar(new FirmaInternaPdfCommand(8, 4), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be("APP_SOL_015");
    }

    [Fact]
    public async Task FirmaPdf_Sin_Detalle_Del_Usuario_Debe_Lanzar_APP_SOL_006()
    {
        var lecturas = new Mock<ISolicitudReadPort>();
        lecturas.Setup(l => l.ObtenerDocumentoParaFirmaInternaMasivaRead(8, 4, It.IsAny<CancellationToken>(), true))
            .ReturnsAsync(Documento());

        var act = () => Pdf(lecturas).Firmar(new FirmaInternaPdfCommand(8, 4), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be("APP_SOL_006");
    }

    [Fact]
    public async Task FirmaPdf_Sin_Archivo_Debe_Lanzar_APP_SOL_009()
    {
        var documento = Documento(conDetalleDelUsuario: true);
        var lecturas = new Mock<ISolicitudReadPort>();
        var archivos = new Mock<IFileStorageService>();
        lecturas.Setup(l => l.ObtenerDocumentoParaFirmaInternaMasivaRead(8, 4, It.IsAny<CancellationToken>(), true))
            .ReturnsAsync(documento);
        archivos.Setup(a => a.GetFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Stream)null!);

        var act = () => Pdf(lecturas, archivos).Firmar(new FirmaInternaPdfCommand(8, 4), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be("APP_SOL_009");
    }

    [Fact]
    public async Task FirmaZip_Sin_Documentos_Debe_Lanzar_APP_SOL_015()
    {
        var lecturas = new Mock<ISolicitudReadPort>();
        lecturas.Setup(l => l.ObtenerDocumentosParaFirmaInternaMasivaRead(8, It.IsAny<List<long>>(), It.IsAny<CancellationToken>(), true))
            .ReturnsAsync([]);

        var act = () => Zip(lecturas).Firmar(new FirmaInternaZipCommand(8, [4]), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be("APP_SOL_015");
    }

    [Fact]
    public async Task FirmaZip_Sin_Archivo_Debe_Lanzar_APP_SOL_009()
    {
        var lecturas = new Mock<ISolicitudReadPort>();
        var archivos = new Mock<IFileStorageService>();
        lecturas.Setup(l => l.ObtenerDocumentosParaFirmaInternaMasivaRead(8, It.IsAny<List<long>>(), It.IsAny<CancellationToken>(), true))
            .ReturnsAsync([Documento()]);
        archivos.Setup(a => a.GetFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Stream)null!);

        var act = () => Zip(lecturas, archivos).Firmar(new FirmaInternaZipCommand(8, [4]), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be("APP_SOL_009");
    }

    [Fact]
    public async Task FirmaExterna_Sin_Documento_Debe_Lanzar_APP_SOL_008()
    {
        var documentos = new Mock<ISolicitudDocumentoRepository>();
        documentos.Setup(d => d.ObtenerSolicitudDocumentoParaFirmadoInterno(4, It.IsAny<CancellationToken>(), true))
            .ReturnsAsync((SolicitudDocumento?)null);

        var act = () => Externa(documentos).Handle(new FirmaExternaCommand(8, 4, Archivo()), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be("APP_SOL_008");
    }

    [Fact]
    public async Task FirmaExterna_Sin_Archivo_En_Storage_Debe_Lanzar_APP_SOL_014()
    {
        var documentos = new Mock<ISolicitudDocumentoRepository>();
        var archivos = new Mock<IFileStorageService>();
        documentos.Setup(d => d.ObtenerSolicitudDocumentoParaFirmadoInterno(4, It.IsAny<CancellationToken>(), true))
            .ReturnsAsync(Documento());
        archivos.Setup(a => a.ExistsFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var act = () => Externa(documentos, archivos).Handle(new FirmaExternaCommand(8, 4, Archivo()), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be("APP_SOL_014");
    }

    [Fact]
    public async Task FirmaExterna_Sin_Detalle_Debe_Lanzar_APP_SOL_006()
    {
        var documentos = new Mock<ISolicitudDocumentoRepository>();
        var archivos = new Mock<IFileStorageService>();
        documentos.Setup(d => d.ObtenerSolicitudDocumentoParaFirmadoInterno(4, It.IsAny<CancellationToken>(), true))
            .ReturnsAsync(Documento());
        documentos.Setup(d => d.ObtenerDetalleFirmaPorDocumentoAsignado(4, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DetalleFirma?)null);
        archivos.Setup(a => a.ExistsFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        archivos.Setup(a => a.GetFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new MemoryStream([1]));

        var act = () => Externa(documentos, archivos).Handle(new FirmaExternaCommand(8, 4, Archivo()), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be("APP_SOL_006");
    }

    [Fact]
    public async Task FirmaExterna_Con_Validacion_Fallida_Debe_Lanzar_APP_SOL_007()
    {
        var (documento, detalle) = DocumentoFirmable();
        var documentos = new Mock<ISolicitudDocumentoRepository>();
        var archivos = new Mock<IFileStorageService>();
        var validador = new Mock<IDocumentoFirmadoValidator>();
        PrepararExterna(documentos, archivos, documento, detalle);
        validador.Setup(v => v.Externo_ValidarDocumentoFirmadoAsync(
                It.IsAny<Stream>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<bool>()))
            .ReturnsAsync(ResultadoValidacionFirma.Fail("dni distinto"));

        var act = () => Externa(documentos, archivos, validador).Handle(new FirmaExternaCommand(8, 4, Archivo()), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be("APP_SOL_007");
    }

    [Fact]
    public async Task FirmaExterna_Valida_Debe_Firmar_El_Detalle()
    {
        var (documento, detalle) = DocumentoFirmable();
        var documentos = new Mock<ISolicitudDocumentoRepository>();
        var archivos = new Mock<IFileStorageService>();
        var validador = new Mock<IDocumentoFirmadoValidator>();
        var hash = new Mock<IPdfContentHashService>();
        var firmas = new Mock<IDetalleFirmaRepository>();
        PrepararExterna(documentos, archivos, documento, detalle);
        validador.Setup(v => v.Externo_ValidarDocumentoFirmadoAsync(
                It.IsAny<Stream>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<bool>()))
            .ReturnsAsync(ResultadoValidacionFirma.Ok());
        hash.Setup(h => h.ObtenerHashContenido(It.IsAny<byte[]>())).Returns("hash-firmado");
        archivos.Setup(a => a.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("guardado");
        firmas.Setup(f => f.UpdateAsync(detalle)).Returns(Task.CompletedTask);

        var resultado = await Externa(documentos, archivos, validador, hash, firmas)
            .Handle(new FirmaExternaCommand(8, 4, Archivo()), CancellationToken.None);

        resultado.Should().BeEmpty();
        detalle.EstadoDocumento.Should().Be(EstadoDocumento.Firmado);
        detalle.HashDocumentoFirmado.Should().Be("hash-firmado");
        detalle.InternoExterno!.Codigo.Should().Be('E');
    }

    private FirmaInternaPdfUseCase Pdf(Mock<ISolicitudReadPort> lecturas, Mock<IFileStorageService>? archivos = null) =>
        new(
            Usuario().Object,
            lecturas.Object,
            (archivos ?? new Mock<IFileStorageService>()).Object,
            new Mock<IDocumentoFirmadoValidator>().Object,
            new Mock<IDetalleFirmaRepository>().Object,
            new Mock<IFirmaPort>().Object,
            new Mock<IFirmaPreparationService>().Object,
            new Mock<IOrientacionPaginaPdfService>().Object,
            new Mock<ILogger<FirmaInternaPdfUseCase>>().Object);

    private static FirmaInternaZipUseCase Zip(Mock<ISolicitudReadPort> lecturas, Mock<IFileStorageService>? archivos = null) =>
        new(
            Usuario().Object,
            lecturas.Object,
            (archivos ?? new Mock<IFileStorageService>()).Object,
            new Mock<IZipStreamingService>().Object,
            new Mock<IFirmaPreparationService>().Object,
            new Mock<IFirmaPort>().Object,
            new Mock<IOrientacionPaginaPdfService>().Object);

    private FirmaExternaCommandHandler Externa(
        Mock<ISolicitudDocumentoRepository> documentos,
        Mock<IFileStorageService>? archivos = null,
        Mock<IDocumentoFirmadoValidator>? validador = null,
        Mock<IPdfContentHashService>? hash = null,
        Mock<IDetalleFirmaRepository>? firmas = null) =>
        new(
            _mapper.Object,
            Usuario().Object,
            (validador ?? new Mock<IDocumentoFirmadoValidator>()).Object,
            documentos.Object,
            (firmas ?? new Mock<IDetalleFirmaRepository>()).Object,
            (archivos ?? new Mock<IFileStorageService>()).Object,
            (hash ?? new Mock<IPdfContentHashService>()).Object);

    private static void PrepararExterna(
        Mock<ISolicitudDocumentoRepository> documentos,
        Mock<IFileStorageService> archivos,
        SolicitudDocumento documento,
        DetalleFirma detalle)
    {
        documentos.Setup(d => d.ObtenerSolicitudDocumentoParaFirmadoInterno(4, It.IsAny<CancellationToken>(), true))
            .ReturnsAsync(documento);
        documentos.Setup(d => d.ObtenerDetalleFirmaPorDocumentoAsignado(4, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(detalle);
        archivos.Setup(a => a.ExistsFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        archivos.Setup(a => a.GetFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new MemoryStream([1]));
    }

    private static SolicitudDocumento Documento(bool conDetalleDelUsuario = false)
    {
        var solicitud = Solicitud.CrearBorrador(8, "GTI-SOL-00000008-2026", 1, 2, 10, 3, 10, 5, 2,
            "Contrato", null, false, false, null, 0);
        solicitud.AgregarDocumento(4, "contrato", "pdf", "originales", "hash-original",
            null, null, null, null, null, null, null, null, 10);
        var documento = solicitud.Documentos.Single();
        documento.Solicitud = solicitud;
        if (conDetalleDelUsuario)
        {
            var detalle = DetalleFirma.Crear(1, 2, 4, null, null, null, null, 10);
            detalle.SolicitudDocumento = documento;
            documento.AgregarDetalleFirma(detalle);
        }

        return documento;
    }

    private static (SolicitudDocumento Documento, DetalleFirma Detalle) DocumentoFirmable()
    {
        var documento = Documento();
        var detalle = DetalleFirma.Crear(1, 2, 4, null, null, null, null, 10);
        detalle.SolicitudDocumento = documento;
        documento.AgregarDetalleFirma(detalle);
        return (documento, detalle);
    }

    private static IFormFile Archivo()
    {
        var archivo = new Mock<IFormFile>();
        archivo.Setup(a => a.OpenReadStream()).Returns(() => new MemoryStream([9, 9]));
        return archivo.Object;
    }

    private static Mock<ICurrentUserService> Usuario()
    {
        var usuario = new Mock<ICurrentUserService>();
        usuario.Setup(u => u.UserIdLocal).Returns(10);
        usuario.Setup(u => u.UserDni).Returns("12345678");
        usuario.Setup(u => u.CodigoUsuario).Returns("USR01");
        return usuario;
    }
}
