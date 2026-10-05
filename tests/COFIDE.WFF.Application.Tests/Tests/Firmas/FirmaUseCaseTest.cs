using COFIDE.WFF.Application.Common.Ports.Out;
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
using Microsoft.Extensions.Logging;
using Moq;
using AppEx = COFIDE.WFF.Application.Exceptions;

using COFIDE.WFF.Application.Tests.Support;
namespace COFIDE.WFF.Application.Tests.Firmas;

public class FirmaUseCaseTest
{

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

    private static SolicitudDocumento Documento(bool conDetalleDelUsuario = false)
    {
        var solicitud = Solicitud.CrearBorrador(8, "GTI-SOL-00000008-2026", 1, 2, 10, 3, 10, 5, 2,
            "Contrato", null, false, false, null, 0);
        solicitud.AgregarDocumento(4, "contrato", "pdf", "originales", "hash-original",
            null, null, null, null, null, null, null, null, 10);
        var documento = solicitud.Documentos.Single();
        NavegacionEf.Asignar(documento, x => x.Solicitud, solicitud);
        if (conDetalleDelUsuario)
        {
            var detalle = DetalleFirma.Crear(1, 2, 4, null, null, null, null, 10);
            NavegacionEf.Asignar(detalle, x => x.SolicitudDocumento, documento);
            documento.AgregarDetalleFirma(detalle);
        }

        return documento;
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
