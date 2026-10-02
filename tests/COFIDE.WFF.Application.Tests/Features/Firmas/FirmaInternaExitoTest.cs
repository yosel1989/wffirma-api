using System.Reflection;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Firmas.Commands.FirmaInternaPdf;
using COFIDE.WFF.Application.Features.Firmas.Commands.FirmaInternaZip;
using COFIDE.WFF.Application.Features.Firmas.DTOs;
using COFIDE.WFF.Application.Features.Firmas.Ports.Out;
using COFIDE.WFF.Application.Features.Firmas.Services;
using COFIDE.WFF.Application.Features.Firmas.UseCases;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Common.Enums;
using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Domain.Exceptions;
using VoConst=COFIDE.WFF.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace COFIDE.WFF.Application.Tests.Features.Firmas;

public class FirmaInternaExitoTest
{
    [Fact]
    public async Task Pdf_Debe_Bloquear_El_Detalle_Y_Guardar_El_Codigo_De_Proceso()
    {
        var (documento, detalle) = DocumentoPendiente();
        var lecturas = new Mock<ISolicitudReadPort>();
        var archivos = new Mock<IFileStorageService>();
        var preparacion = new Mock<IFirmaPreparationService>();
        var puerto = new Mock<IFirmaPort>();
        var orientacion = new Mock<IOrientacionPaginaPdfService>();
        var firmas = new Mock<IDetalleFirmaRepository>();
        var solicitud = new FirmarDocumentoRequestDto { TiempoEspera = 5000 };
        var respuesta = new FirmaInternaResponseDto { CodigoProceso = "PROC-1", UrlFirma = "https://firma" };

        lecturas.Setup(l => l.ObtenerDocumentoParaFirmaInternaMasivaRead(8, 4, It.IsAny<CancellationToken>(), true))
            .ReturnsAsync(documento);
        archivos.Setup(a => a.GetFileAsync(Path.Combine("originales", "contrato.pdf"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new MemoryStream([1, 2, 3]));
        orientacion.Setup(o => o.ObtenerPrimeraPagina(It.IsAny<Stream>()))
            .Returns(new InformacionPaginaPdfDto(100, 200, false));
        preparacion.Setup(p => p.PrepararDocumentoFirmaAsync(
                documento, detalle.CodigoReferencial!, 10, 1, It.IsAny<InformacionPaginaPdfDto?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(solicitud);
        puerto.Setup(p => p.FirmarDocumentoAsync(solicitud, It.IsAny<CancellationToken>())).ReturnsAsync(respuesta);
        firmas.Setup(f => f.UpdateAsync(detalle)).Returns(Task.CompletedTask);

        var resultado = await new FirmaInternaPdfUseCase(
            Usuario(),
            lecturas.Object,
            archivos.Object,
            new Mock<IDocumentoFirmadoValidator>().Object,
            firmas.Object,
            puerto.Object,
            preparacion.Object,
            orientacion.Object,
            NullLogger<FirmaInternaPdfUseCase>.Instance)
            .Firmar(new FirmaInternaPdfCommand(8, 4), CancellationToken.None);

        resultado!.CodigoProceso.Should().Be("PROC-1");
        resultado.TiempoEspera.Should().Be(5000);
        detalle.EstadoDocumento.Should().Be(VoConst.EstadoDocumento.Bloqueado);
        detalle.CodigoFirma.Should().Be("PROC-1");
        detalle.FechaInicioProceso.Should().NotBeNull();
        firmas.Verify(f => f.UpdateAsync(detalle), Times.Once);
    }

    [Fact]
    public async Task Pdf_Con_Detalle_Ya_Firmado_Debe_Fallar_Antes_De_Leer_El_Archivo()
    {
        var (documento, detalle) = DocumentoPendiente();
        var firmante = new Usuario(10, RolUsuario.SolicitanteAprobador);
        typeof(Usuario).GetField("<NumeroDocumento>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(firmante, "12345678");
        detalle.UsuarioFirmante = firmante;
        detalle.Firmar("contrato [F]", "pdf", "firmados", "hash", true);
        var lecturas = new Mock<ISolicitudReadPort>();
        var archivos = new Mock<IFileStorageService>();
        lecturas.Setup(l => l.ObtenerDocumentoParaFirmaInternaMasivaRead(8, 4, It.IsAny<CancellationToken>(), true))
            .ReturnsAsync(documento);

        var act = () => new FirmaInternaPdfUseCase(
            Usuario(),
            lecturas.Object,
            archivos.Object,
            new Mock<IDocumentoFirmadoValidator>().Object,
            new Mock<IDetalleFirmaRepository>().Object,
            new Mock<IFirmaPort>().Object,
            new Mock<IFirmaPreparationService>().Object,
            new Mock<IOrientacionPaginaPdfService>().Object,
            NullLogger<FirmaInternaPdfUseCase>.Instance)
            .Firmar(new FirmaInternaPdfCommand(8, 4), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
        archivos.Verify(a => a.GetFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Zip_Debe_Bloquear_Los_Detalles_Y_Enviar_El_Lote()
    {
        var (documento, detalle) = DocumentoPendiente();
        var lecturas = new Mock<ISolicitudReadPort>();
        var archivos = new Mock<IFileStorageService>();
        var zip = new Mock<IZipStreamingService>();
        var preparacion = new Mock<IFirmaPreparationService>();
        var puerto = new Mock<IFirmaPort>();
        var orientacion = new Mock<IOrientacionPaginaPdfService>();
        var solicitud = new FirmarDocumentoRequestDto { TiempoEspera = 5 };
        var respuesta = new FirmaInternaResponseDto { CodigoProceso = "LOTE-1" };

        lecturas.Setup(l => l.ObtenerDocumentosParaFirmaInternaMasivaRead(8, It.IsAny<List<long>>(), It.IsAny<CancellationToken>(), true))
            .ReturnsAsync(new List<SolicitudDocumento> { documento });
        archivos.Setup(a => a.GetFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new MemoryStream([1, 2, 3]));
        orientacion.Setup(o => o.ObtenerPrimeraPagina(It.IsAny<Stream>()))
            .Returns(new InformacionPaginaPdfDto(100, 200, false));
        preparacion.Setup(p => p.PrepararDocumentoFirmaAsync(
                documento, "GTI-SOL-00000008-2026", 10, 1, It.IsAny<InformacionPaginaPdfDto?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(solicitud);
        zip.Setup(z => z.StreamZipAsync(It.IsAny<Stream>(), It.IsAny<IReadOnlyCollection<FirmaZipRequestDto>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        puerto.Setup(p => p.FirmarBatchAsync(solicitud, It.IsAny<CancellationToken>())).ReturnsAsync(respuesta);

        var resultado = await new FirmaInternaZipUseCase(
            Usuario(),
            lecturas.Object,
            archivos.Object,
            zip.Object,
            preparacion.Object,
            puerto.Object,
            orientacion.Object)
            .Firmar(new FirmaInternaZipCommand(8, [4]), CancellationToken.None);

        resultado!.CodigoProceso.Should().Be("LOTE-1");
        resultado.TiempoEspera.Should().Be(5);
        detalle.EstadoDocumento.Should().Be(VoConst.EstadoDocumento.Bloqueado);
        detalle.CodigoFirma.Should().Be("LOTE-1");
        zip.Verify(z => z.StreamZipAsync(
            It.IsAny<Stream>(), It.IsAny<IReadOnlyCollection<FirmaZipRequestDto>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private static (SolicitudDocumento Documento, DetalleFirma Detalle) DocumentoPendiente()
    {
        var solicitud = Solicitud.CrearBorrador(8, "GTI-SOL-00000008-2026", 1, 2, 10, 3, 10, 5, 2,
            "Contrato", null, false, false, null, 0);
        solicitud.AgregarDocumento(4, "contrato", "pdf", "originales", "hash-original",
            null, null, null, null, null, null, null, null, 10);
        var documento = solicitud.Documentos.Single();
        documento.Solicitud = solicitud;
        var detalle = DetalleFirma.Crear(1, 2, 4, null, null, null, null, 10);
        detalle.SolicitudDocumento = documento;
        documento.AgregarDetalleFirma(detalle);
        return (documento, detalle);
    }

    private static ICurrentUserService Usuario()
    {
        var usuario = new Mock<ICurrentUserService>();
        usuario.Setup(u => u.UserIdLocal).Returns(10L);
        return usuario.Object;
    }
}
