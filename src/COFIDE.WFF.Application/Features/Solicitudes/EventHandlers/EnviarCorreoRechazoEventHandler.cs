using COFIDE.WFF.Application.Common.Events;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using COFIDE.WFF.Application.Common.Constants;
using COFIDE.WFF.Domain.Common.Tiempo;
namespace COFIDE.WFF.Application.Features.Solicitudes.EventHandlers;

public class EnviarCorreoRechazoEventHandler(
        ISolicitudQueryRepository solicitudQueryRepository,
        ISolicitudPlantillaRepository solicitudPlantillaQueryRepository,
        ICorreoSenderPort correoSenderPort,
        IFormatoNotificacionRepository formatoNotificacionRepository,
        IHtmlTemplateRenderer htmlTemplateRenderer,
        IUrlProvider urlProvider,
        ISolicitudIdProtector solicitudIdProtector,
        ILogger<EnviarCorreoRechazoEventHandler> logger
    ) : INotificationHandler<DomainEventNotification<SolicitudRechazadaDomainEvent>>
{
    private readonly ISolicitudQueryRepository _solicitudQueryRepository = solicitudQueryRepository;
    private readonly ISolicitudPlantillaRepository _solicitudPlantillaQueryRepository = solicitudPlantillaQueryRepository;
    private readonly ICorreoSenderPort _correoSenderPort = correoSenderPort;
    private readonly IFormatoNotificacionRepository _formatoNotificacionRepository = formatoNotificacionRepository;
    private readonly IHtmlTemplateRenderer _htmlTemplateRenderer = htmlTemplateRenderer;
    private readonly IUrlProvider _urlProvider = urlProvider;
    private readonly ISolicitudIdProtector _solicitudIdProtector = solicitudIdProtector;
    private readonly ILogger<EnviarCorreoRechazoEventHandler> _logger = logger;

    public async Task Handle(DomainEventNotification<SolicitudRechazadaDomainEvent> notification, CancellationToken cancellationToken)
    {
        try
        {
            var baseFrontUrl = _urlProvider.GetWorkflowBaseFrontUrl().TrimEnd('/');
            var domainEvent = notification.DomainEvent;

            var templateCorreo = await _formatoNotificacionRepository
                .ObtenerPorCodigo(CodigosPlantillaCorreo.SolicitudRechazada, cancellationToken)
                ?? throw new InvalidOperationException("Template SOLICITUD_RECHAZADA no configurado.");

            var solicitud = await _solicitudQueryRepository
                .ObtenerParaNotificaciones(domainEvent.IdSolicitud, cancellationToken)
                ?? throw new InvalidOperationException("Solicitud no encontrada para notificaciones.");

            var etapaActual = await _solicitudPlantillaQueryRepository
                .ObtenerEtapaActual(domainEvent.IdSolicitud, 0, cancellationToken)
                ?? throw new InvalidOperationException("No se encontró etapa actual.");

            var tokenSolicitud = _solicitudIdProtector.Encriptar(
                domainEvent.IdSolicitud);

            var tokenUrl = Uri.EscapeDataString(tokenSolicitud);

            var modeloCorreoCuerpo = new
            {
                solicitud.TipoSolicitud,
                solicitud.CodigoSolicitud,
                solicitud.NombreSolicitante,
                solicitud.SubTipoSolicitud,
                solicitud.TipoOperacion,
                solicitud.AsuntoSolicitud,
                solicitud.FechaCreacion,
                solicitud.MotivoRechazo,
                NombreAprobadorRechazo = domainEvent.nombreAprobador,
                etapaActual.NombreEtapa,
                Anio = HoraSistema.Ahora.Year.ToString(),
                FechaRechazo = HoraSistema.Ahora.ToString("dd/MM/yyyy"),
                LinkSolicitud = $"{baseFrontUrl}/mis_solicitudes/detalle/visualizar?token={tokenUrl}"
            };

            var asunto = _htmlTemplateRenderer.Render(
                templateCorreo.AsuntoCorreo ?? string.Empty, solicitud);

            var cuerpo = _htmlTemplateRenderer.Render(
                templateCorreo.CuerpoMensaje ?? string.Empty, modeloCorreoCuerpo);

            await _correoSenderPort.EnviarCorreoAsync(
                asunto, cuerpo,
                solicitud.correoSolicitante,
                templateCorreo.CorreoRemitente ?? string.Empty,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al notificar un correo borrador");
        }
    }
}