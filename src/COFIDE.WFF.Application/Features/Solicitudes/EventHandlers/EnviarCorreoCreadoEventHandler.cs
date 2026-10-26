using COFIDE.WFF.Application.Common.Events;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using COFIDE.WFF.Application.Common.Constants;
using COFIDE.WFF.Domain.Common.Tiempo;
namespace COFIDE.WFF.Application.Features.Solicitudes.EventHandlers;

public class EnviarCorreoCreadoEventHandler(
        ISolicitudQueryRepository solicitudQueryRepository,
        ICorreoSenderPort correoSenderPort,
        IFormatoNotificacionRepository formatoNotificacionRepository,
        IHtmlTemplateRenderer htmlTemplateRenderer,
        IUrlProvider urlProvider,
        ILogger<EnviarCorreoCreadoEventHandler> logger
    ) : INotificationHandler<DomainEventNotification<SolicitudCreadaDomainEvent>>
{
    private readonly ISolicitudQueryRepository _solicitudQueryRepository = solicitudQueryRepository;
    private readonly ICorreoSenderPort _correoSenderPort = correoSenderPort;
    private readonly IFormatoNotificacionRepository _formatoNotificacionRepository = formatoNotificacionRepository;
    private readonly IHtmlTemplateRenderer _htmlTemplateRenderer = htmlTemplateRenderer;
    private readonly IUrlProvider _urlProvider = urlProvider;
    private readonly ILogger<EnviarCorreoCreadoEventHandler> _logger = logger;

    public async Task Handle(DomainEventNotification<SolicitudCreadaDomainEvent> notification, CancellationToken cancellationToken)
    {
        try
        {
            var baseFrontUrl = _urlProvider.GetWorkflowBaseFrontUrl().TrimEnd('/');
            var domainEvent = notification.DomainEvent;

            var templateCorreo = await _formatoNotificacionRepository
                .ObtenerPorCodigo(CodigosPlantillaCorreo.SolicitudEnvio, cancellationToken)
                ?? throw new InvalidOperationException("Template SOLICITUD_ENVIO no configurado.");

            var solicitud = await _solicitudQueryRepository
                .ObtenerParaNotificaciones(domainEvent.IdSolicitud, cancellationToken)
                ?? throw new InvalidOperationException("Solicitud no encontrada para notificaciones.");

            var modeloCorreoCuerpo = new
            {
                solicitud.TipoSolicitud,
                solicitud.CodigoSolicitud,
                solicitud.NombreSolicitante,
                solicitud.SubTipoSolicitud,
                solicitud.TipoOperacion,
                solicitud.AsuntoSolicitud,
                solicitud.FechaCreacion,
                Anio = HoraSistema.Ahora.Year.ToString(),
                LinkSolicitud = $"{baseFrontUrl}/mis_solicitudes/detalle/editar?id={domainEvent.IdSolicitud}"
            };

            var asunto = _htmlTemplateRenderer.Render(
                templateCorreo.AsuntoCorreo ?? string.Empty, solicitud);

            var cuerpo = _htmlTemplateRenderer.Render(
                templateCorreo.CuerpoMensaje ?? string.Empty, modeloCorreoCuerpo);

            await _correoSenderPort.EnviarCorreoAsync(
                asunto, cuerpo,
                domainEvent.correoSolicitante ?? string.Empty,
                templateCorreo.CorreoRemitente ?? string.Empty,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al notificar un correo borrador");
        }
    }
}
