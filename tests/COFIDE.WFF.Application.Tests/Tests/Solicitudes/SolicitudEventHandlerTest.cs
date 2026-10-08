using COFIDE.WFF.Application.Common.Events;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Firmas.EventHandlers;
using COFIDE.WFF.Application.Features.Plantillas.Queries.ObtenerPorId;
using COFIDE.WFF.Application.Features.Solicitudes.EventHandlers;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Events;
using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace COFIDE.WFF.Application.Tests.Solicitudes;

public class SolicitudEventHandlerTest
{
    [Fact]
    public async Task DocumentoFirmado_Debe_Notificar_El_Hub_Con_Estado_En_Curso()
    {
        var puerto = new Mock<INotificacionTiempoRealPort>();
        var evento = new DocumentoFirmadoDomainEvent(1, 20, 30, true, "USR01", "ok");

        await new DocumentoFirmadoHubEventHandler(puerto.Object)
            .Handle(new DomainEventNotification<DocumentoFirmadoDomainEvent>(evento), CancellationToken.None);

        puerto.Verify(p => p.NotificarFirmaDocumentoAsync(
            20, 30, "2", true, "USR01", "ok", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CorreoBorrador_Sin_Plantilla_No_Debe_Propagar_La_Excepcion()
    {
        var formatos = new Mock<IFormatoNotificacionRepository>();
        var correos = new Mock<ICorreoSenderPort>();
        formatos.Setup(f => f.ObtenerPorCodigo("SOLICITUD_BORRADOR", It.IsAny<CancellationToken>()))
            .ReturnsAsync((FormatoNotificacion?)null);

        var act = () => new EnviarCorreoBorradorEventHandler(
            new Mock<ISolicitudQueryRepository>().Object,
            correos.Object,
            formatos.Object,
            new Mock<IHtmlTemplateRenderer>().Object,
            new Mock<IUrlProvider>().Object,
            new Mock<ILogger<EnviarCorreoBorradorEventHandler>>().Object)
            .Handle(NotificacionBorrador(), CancellationToken.None);

        await act.Should().NotThrowAsync();
        correos.Verify(c => c.EnviarCorreoAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CorreoBorrador_Debe_Enviar_Al_Correo_Del_Solicitante()
    {
        var solicitudes = new Mock<ISolicitudQueryRepository>();
        var correos = new Mock<ICorreoSenderPort>();
        var formatos = new Mock<IFormatoNotificacionRepository>();
        var renderer = new Mock<IHtmlTemplateRenderer>();
        var urls = new Mock<IUrlProvider>();

        urls.Setup(u => u.GetWorkflowBaseFrontUrl()).Returns("https://firmas.cofide");
        formatos.Setup(f => f.ObtenerPorCodigo("SOLICITUD_BORRADOR", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FormatoNotificacionPrueba());
        solicitudes.Setup(s => s.ObtenerParaNotificaciones(8, It.IsAny<CancellationToken>()))
            .ReturnsAsync(NotificacionSolicitud());
        renderer.Setup(r => r.Render(It.IsAny<string>(), It.IsAny<object>())).Returns("render");

        await new EnviarCorreoBorradorEventHandler(
            solicitudes.Object, correos.Object, formatos.Object, renderer.Object, urls.Object,
            new Mock<ILogger<EnviarCorreoBorradorEventHandler>>().Object)
            .Handle(NotificacionBorrador(), CancellationToken.None);

        correos.Verify(c => c.EnviarCorreoAsync(
            "render", "render", "ana@cofide.com.pe", string.Empty, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CorreoCreado_Debe_Enviar_Al_Correo_Del_Evento()
    {
        var (solicitudes, correos, formatos, renderer, urls) = CorreoListo("SOLICITUD_ENVIO");

        await new EnviarCorreoCreadoEventHandler(
            solicitudes.Object, correos.Object, formatos.Object, renderer.Object, urls.Object,
            new Mock<ILogger<EnviarCorreoCreadoEventHandler>>().Object)
            .Handle(
                new DomainEventNotification<SolicitudCreadaDomainEvent>(
                    new SolicitudCreadaDomainEvent(8, "ana@cofide.com.pe")),
                CancellationToken.None);

        correos.Verify(c => c.EnviarCorreoAsync(
            "render", "render", "ana@cofide.com.pe", string.Empty, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CorreoRechazo_Sin_Etapa_Actual_No_Debe_Enviar()
    {
        var (solicitudes, correos, formatos, renderer, urls) = CorreoListo("SOLICITUD_RECHAZADA");
        var plantillas = new Mock<ISolicitudPlantillaRepository>();
        plantillas.Setup(p => p.ObtenerEtapaActual(8, 0, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlantillaEtapaDto?)null);

        var act = () => new EnviarCorreoRechazoEventHandler(
            solicitudes.Object, plantillas.Object, correos.Object, formatos.Object,
            renderer.Object, urls.Object, new Mock<ILogger<EnviarCorreoRechazoEventHandler>>().Object)
            .Handle(
                new DomainEventNotification<SolicitudRechazadaDomainEvent>(
                    new SolicitudRechazadaDomainEvent(8, "Luis")),
                CancellationToken.None);

        await act.Should().NotThrowAsync();
        correos.Verify(c => c.EnviarCorreoAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CorreoRechazo_Debe_Enviar_Al_Correo_De_La_Solicitud()
    {
        var (solicitudes, correos, formatos, renderer, urls) = CorreoListo("SOLICITUD_RECHAZADA");
        var plantillas = new Mock<ISolicitudPlantillaRepository>();
        plantillas.Setup(p => p.ObtenerEtapaActual(8, 0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PlantillaEtapaDto { NombreEtapa = "Etapa 1" });

        await new EnviarCorreoRechazoEventHandler(
            solicitudes.Object, plantillas.Object, correos.Object, formatos.Object,
            renderer.Object, urls.Object, new Mock<ILogger<EnviarCorreoRechazoEventHandler>>().Object)
            .Handle(
                new DomainEventNotification<SolicitudRechazadaDomainEvent>(
                    new SolicitudRechazadaDomainEvent(8, "Luis")),
                CancellationToken.None);

        correos.Verify(c => c.EnviarCorreoAsync(
            "render", "render", "ana@cofide.com.pe", string.Empty, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RegistrarEvento_Debe_Persistir_Dentro_De_Una_Transaccion()
    {
        var eventos = new Mock<ISolicitudEventoRepository>();
        var uow = new Mock<IUnitOfWork>();
        eventos.Setup(e => e.ObtenerSiguienteIdEventoAsync(It.IsAny<CancellationToken>())).ReturnsAsync(50);
        eventos.Setup(e => e.AddAsync(It.IsAny<SolicitudEvento>())).Returns(Task.CompletedTask);

        await new RegistrarEventoSolicitudEventHandler(
            eventos.Object, uow.Object, new Mock<ILogger<RegistrarEventoSolicitudEventHandler>>().Object)
            .Handle(NotificacionEvento(), CancellationToken.None);

        uow.Verify(u => u.BeginTransactionAsync(), Times.Once);
        uow.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        uow.Verify(u => u.RollbackTransactionAsync(), Times.Never);
        eventos.Verify(e => e.AddAsync(It.Is<SolicitudEvento>(x => x.Id == 50)), Times.Once);
    }

    [Fact]
    public async Task RegistrarEvento_Cuando_Falla_Debe_Hacer_Rollback_Y_No_Propagar()
    {
        var eventos = new Mock<ISolicitudEventoRepository>();
        var uow = new Mock<IUnitOfWork>();
        eventos.Setup(e => e.ObtenerSiguienteIdEventoAsync(It.IsAny<CancellationToken>())).ReturnsAsync(50);
        eventos.Setup(e => e.AddAsync(It.IsAny<SolicitudEvento>())).ThrowsAsync(new InvalidOperationException("db"));

        var act = () => new RegistrarEventoSolicitudEventHandler(
            eventos.Object, uow.Object, new Mock<ILogger<RegistrarEventoSolicitudEventHandler>>().Object)
            .Handle(NotificacionEvento(), CancellationToken.None);

        await act.Should().NotThrowAsync();
        uow.Verify(u => u.RollbackTransactionAsync(), Times.Once);
        uow.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static DomainEventNotification<SolicitudCreadaBorradorDomainEvent> NotificacionBorrador() =>
        new(new SolicitudCreadaBorradorDomainEvent(8, "ana@cofide.com.pe"));

    private static DomainEventNotification<SolicitudEventoRegistradoDomainEvent> NotificacionEvento() =>
        new(new SolicitudEventoRegistradoDomainEvent(8, 1, 10, null, null, "GTI", null));

    private static (
        Mock<ISolicitudQueryRepository> Solicitudes,
        Mock<ICorreoSenderPort> Correos,
        Mock<IFormatoNotificacionRepository> Formatos,
        Mock<IHtmlTemplateRenderer> Renderer,
        Mock<IUrlProvider> Urls) CorreoListo(string codigoPlantilla)
    {
        var solicitudes = new Mock<ISolicitudQueryRepository>();
        var correos = new Mock<ICorreoSenderPort>();
        var formatos = new Mock<IFormatoNotificacionRepository>();
        var renderer = new Mock<IHtmlTemplateRenderer>();
        var urls = new Mock<IUrlProvider>();

        urls.Setup(u => u.GetWorkflowBaseFrontUrl()).Returns("https://firmas.cofide");
        formatos.Setup(f => f.ObtenerPorCodigo(codigoPlantilla, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FormatoNotificacionPrueba());
        solicitudes.Setup(s => s.ObtenerParaNotificaciones(8, It.IsAny<CancellationToken>()))
            .ReturnsAsync(NotificacionSolicitud());
        renderer.Setup(r => r.Render(It.IsAny<string>(), It.IsAny<object>())).Returns("render");

        return (solicitudes, correos, formatos, renderer, urls);
    }

    private static FormatoNotificacionSolicitud NotificacionSolicitud() =>
        new("Solicitud", "GTI-SOL-00000008-2026", "ana@cofide.com.pe", "Ana",
            "Contrato", "Alta", "Asunto", "2026-10-02", false, null, null, null, false);

    private sealed class FormatoNotificacionPrueba : FormatoNotificacion
    {
    }
}
