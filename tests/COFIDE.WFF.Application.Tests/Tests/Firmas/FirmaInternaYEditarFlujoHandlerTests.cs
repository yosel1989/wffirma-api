using System.Text.Json;
using AutoMapper;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Firmas.Commands.FirmaInternaWebHook;
using COFIDE.WFF.Application.Features.Firmas.Ports.Out;
using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Services;
using COFIDE.WFF.Application.Features.Revisiones.EditarFlujo;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Services;
using COFIDE.WFF.Application.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.DomainServices;
using COFIDE.WFF.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using AppEx = COFIDE.WFF.Application.Exceptions;
using COFIDE.WFF.Application.Tests.Support;

namespace COFIDE.WFF.Application.Tests.Tests.Firmas;

public class FirmaInternaYEditarFlujoHandlerTests
{
    private readonly Mock<IMapper> _mapper = new();

    [Fact]
    public async Task WebHook_Sin_Detalles_Debe_Lanzar_APP_SOL_006()
    {
        var detalles = new Mock<IDetalleFirmaRepository>();
        detalles.Setup(d => d.ObtenerDetalleFirmaPorCodigoFirma("COD-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var act = () => WebHook(detalles).Handle(Comando("[]"), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be("APP_SOL_006");
    }

    [Fact]
    public async Task WebHook_Sin_Documentos_Debe_Lanzar_APP_SOL_008()
    {
        var detalle = DetallePendiente();
        var detalles = new Mock<IDetalleFirmaRepository>();
        detalles.Setup(d => d.ObtenerDetalleFirmaPorCodigoFirma("COD-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync([detalle]);

        var act = () => WebHook(detalles).Handle(Comando("[]"), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be("APP_SOL_008");
    }

    [Fact]
    public async Task WebHook_Ya_Firmado_Debe_Ignorarse()
    {
        var detalle = DetallePendiente();
        detalle.Firmar("contrato [F]", "pdf", "firmados", "hash", true);
        var firma = new Mock<IFirmaPort>();
        var detalles = new Mock<IDetalleFirmaRepository>();
        detalles.Setup(d => d.ObtenerDetalleFirmaPorCodigoFirma("COD-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync([detalle]);

        var resultado = await WebHook(detalles, firma).Handle(
            Comando(JsonDe(detalle.CodigoReferencial!)), CancellationToken.None);

        resultado.Should().Be("OK");
        firma.Verify(f => f.ObtenerDocumentoFirmadoAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task WebHook_Con_Otro_Nombre_Debe_Ignorar_El_Detalle()
    {
        var firma = new Mock<IFirmaPort>();
        var detalles = new Mock<IDetalleFirmaRepository>();
        detalles.Setup(d => d.ObtenerDetalleFirmaPorCodigoFirma("COD-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync([DetallePendiente()]);

        var resultado = await WebHook(detalles, firma).Handle(Comando(JsonDe("otro")), CancellationToken.None);

        resultado.Should().Be("OK");
        firma.Verify(f => f.ObtenerDocumentoFirmadoAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task WebHook_Con_Bytes_Vacios_Debe_Reactivar_El_Documento_Y_Responder_OK()
    {
        var detalle = DetallePendiente();
        var firma = new Mock<IFirmaPort>();
        var detalles = new Mock<IDetalleFirmaRepository>();
        firma.Setup(f => f.ObtenerDocumentoFirmadoAsync(
                It.IsAny<string>(), "COD-1", "doc-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        detalles.Setup(d => d.ObtenerDetalleFirmaPorCodigoFirma("COD-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync([detalle]);
        detalles.Setup(d => d.UpdateAsync(detalle)).Returns(Task.CompletedTask);

        var resultado = await WebHook(detalles, firma).Handle(
            Comando(JsonDe(detalle.CodigoReferencial!)), CancellationToken.None);

        resultado.Should().Be("OK");
        detalle.EstadoDocumento.Should().Be(EstadoDocumento.Pendiente);
        detalles.Verify(d => d.UpdateAsync(detalle), Times.Once);
    }

    [Fact]
    public async Task WebHook_Con_Validacion_Fallida_No_Debe_Guardar_El_Archivo()
    {
        var (detalle, archivos, validador, hash) = Grafo();
        var firma = new Mock<IFirmaPort>();
        var detalles = new Mock<IDetalleFirmaRepository>();
        firma.Setup(f => f.ObtenerDocumentoFirmadoAsync("", "COD-1", "doc-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync([9, 9]);
        validador.Setup(v => v.Interno_ValidarDocumentoFirmadoAsync(It.IsAny<Stream>(), It.IsAny<Stream>()))
            .ReturnsAsync("version distinta");
        detalles.Setup(d => d.ObtenerDetalleFirmaPorCodigoFirma("COD-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync([detalle]);
        detalles.Setup(d => d.UpdateAsync(detalle)).Returns(Task.CompletedTask);

        var resultado = await WebHook(detalles, firma, archivos, validador, hash).Handle(
            Comando(JsonDe(detalle.CodigoReferencial!)), CancellationToken.None);

        resultado.Should().Be("OK");
        detalle.EstadoDocumento.Should().Be(EstadoDocumento.Pendiente);
        archivos.Verify(a => a.SaveFileAsync(
            It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task WebHook_Valido_Debe_Firmar_El_Detalle()
    {
        var (detalle, archivos, validador, hash) = Grafo();
        var firma = new Mock<IFirmaPort>();
        var detalles = new Mock<IDetalleFirmaRepository>();
        firma.Setup(f => f.ObtenerDocumentoFirmadoAsync("", "COD-1", "doc-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync([9, 9]);
        validador.Setup(v => v.Interno_ValidarDocumentoFirmadoAsync(It.IsAny<Stream>(), It.IsAny<Stream>()))
            .ReturnsAsync(string.Empty);
        hash.Setup(h => h.ObtenerHashContenido(It.IsAny<byte[]>())).Returns("hash-firmado");
        archivos.Setup(a => a.SaveFileAsync(
                It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("guardado");
        detalles.Setup(d => d.ObtenerDetalleFirmaPorCodigoFirma("COD-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync([detalle]);
        detalles.Setup(d => d.UpdateAsync(detalle)).Returns(Task.CompletedTask);

        var resultado = await WebHook(detalles, firma, archivos, validador, hash).Handle(
            Comando(JsonDe(detalle.CodigoReferencial!)), CancellationToken.None);

        resultado.Should().Be("OK");
        detalle.EstadoDocumento.Should().Be(EstadoDocumento.Firmado);
        detalle.NombreDocumentoFirmado.Should().Be("contrato [F]");
        detalle.HashDocumentoFirmado.Should().Be("hash-firmado");
        detalles.Verify(d => d.UpdateAsync(detalle), Times.Once);
    }

    [Fact]
    public async Task EditarFlujo_Sin_Solicitud_Debe_Lanzar_APP_SOL_001()
    {
        var solicitudes = new Mock<ISolicitudRepository>();
        solicitudes.Setup(s => s.ObtenerAgregadoSolicitudAsync(8, It.IsAny<CancellationToken>(), true))
            .ReturnsAsync((Solicitud?)null);

        var act = () => Editar(solicitudes).Handle(new EditarFlujoCommand(8, 5, null), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be("APP_SOL_001");
    }

    [Fact]
    public async Task EditarFlujo_Con_Otra_Plantilla_Debe_Lanzar_APP_SOL_005()
    {
        var solicitudes = new Mock<ISolicitudRepository>();
        var plantillas = new Mock<IPlantillaRepository>();
        solicitudes.Setup(s => s.ObtenerAgregadoSolicitudAsync(8, It.IsAny<CancellationToken>(), true))
            .ReturnsAsync(SolicitudConPlantilla(5));

        var act = () => Editar(solicitudes, plantillas).Handle(new EditarFlujoCommand(8, 9, null), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be("APP_SOL_005");
        plantillas.Verify(p => p.ObtenerAgregadoPlantillaAsync(
            It.IsAny<long>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task EditarFlujo_Sin_Plantilla_Debe_Lanzar_APP_SOL_005()
    {
        var solicitudes = new Mock<ISolicitudRepository>();
        var plantillas = new Mock<IPlantillaRepository>();
        solicitudes.Setup(s => s.ObtenerAgregadoSolicitudAsync(8, It.IsAny<CancellationToken>(), true))
            .ReturnsAsync(SolicitudConPlantilla(5));
        plantillas.Setup(p => p.ObtenerAgregadoPlantillaAsync(5, It.IsAny<CancellationToken>(), true))
            .ReturnsAsync((Plantilla?)null);

        var act = () => Editar(solicitudes, plantillas).Handle(new EditarFlujoCommand(8, 5, null), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be("APP_SOL_005");
    }

    private FirmaInternaWebHookCommandHandler WebHook(
        Mock<IDetalleFirmaRepository> detalles,
        Mock<IFirmaPort>? firma = null,
        Mock<IFileStorageService>? archivos = null,
        Mock<IDocumentoFirmadoValidator>? validador = null,
        Mock<IPdfContentHashService>? hash = null)
    {
        var usuario = new Mock<ICurrentUserService>();
        usuario.Setup(u => u.CodigoUsuario).Returns("USR01");
        archivos ??= ArchivosDePrueba.Almacenamiento();
        archivos.Setup(a => a.GetFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new MemoryStream([1, 2, 3]));

        return new FirmaInternaWebHookCommandHandler(
            _mapper.Object,
            usuario.Object,
            (validador ?? new Mock<IDocumentoFirmadoValidator>()).Object,
            (firma ?? new Mock<IFirmaPort>()).Object,
            detalles.Object,
            archivos.Object,
            (hash ?? new Mock<IPdfContentHashService>()).Object,
            new Mock<ILogger<FirmaInternaWebHookCommandHandler>>().Object);
    }

    private EditarFlujoCommandHandler Editar(
        Mock<ISolicitudRepository> solicitudes,
        Mock<IPlantillaRepository>? plantillas = null)
    {
        var usuario = new Mock<ICurrentUserService>();
        usuario.Setup(u => u.UserIdLocal).Returns(10L);
        usuario.Setup(u => u.UserGerenciaNombre).Returns("GTI");

        return new EditarFlujoCommandHandler(
            _mapper.Object,
            usuario.Object,
            solicitudes.Object,
            new Mock<ISolicitudPlantillaRepository>().Object,
            (plantillas ?? new Mock<IPlantillaRepository>()).Object,
            new Mock<IPlantillaCreacionService>().Object,
            new Mock<IWorkflowDomainService>().Object,
            new Mock<IParametroRepository>().Object,
            new FlujoDiffDomainService(),
            new Mock<IGeneradorCorrelativoIdRepository>().Object);
    }

    private static (
        DetalleFirma Detalle,
        Mock<IFileStorageService> Archivos,
        Mock<IDocumentoFirmadoValidator> Validador,
        Mock<IPdfContentHashService> Hash) Grafo()
    {
        var solicitud = Solicitud.CrearBorrador(
            8, "GTI-SOL-00000008-2026", 1, 2, 10, 3, 10, 5, 2,
            "Contrato", null, false, false, null, 0);
        solicitud.AgregarDocumento(
            4, "contrato", "pdf", "originales", "hash-original",
            null, null, null, null, null, null, null, null, 10);
        var documento = solicitud.Documentos.Single();
        documento.Solicitud = solicitud;

        var detalle = DetalleFirma.Crear(1, 2, 4, null, null, null, null, 100);
        detalle.SolicitudDocumento = documento;
        documento.AgregarDetalleFirma(detalle);

        return (detalle, ArchivosDePrueba.Almacenamiento(), new Mock<IDocumentoFirmadoValidator>(), new Mock<IPdfContentHashService>());
    }

    private static DetalleFirma DetallePendiente() =>
        DetalleFirma.Crear(1, 2, 4, null, null, null, null, 100);

    private static Solicitud SolicitudConPlantilla(long idPlantilla)
    {
        var solicitud = Solicitud.CrearBorrador(
            8, "GTI-SOL-00000008-2026", 1, 2, 10, 3, 10, 5, 2,
            "Contrato", null, false, false, null, 0);
        var plantilla = Plantilla.Crear(idPlantilla, "Flujo", "Descripcion", false, 10, "GTI", true);
        var asignada = SolicitudPlantilla.Crear(1, solicitud, plantilla);
        asignada.ActualizarPlantilla(idPlantilla);
        solicitud.AsignarPlantilla(asignada);
        return solicitud;
    }

    private static FirmaInternaWebHookCommand Comando(string documentId) =>
        new("COD-1", documentId, "2026-10-02", "Ana", "12345678");

    private static string JsonDe(string documentName) =>
        JsonSerializer.Serialize(new List<DocumentoFirmadoDto>
        {
            new() { id = "doc-1", documentName = documentName }
        });
}
