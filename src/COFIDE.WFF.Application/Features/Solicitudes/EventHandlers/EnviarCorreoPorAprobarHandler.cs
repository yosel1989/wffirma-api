using COFIDE.WFF.Application.Common.Events;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using COFIDE.WFF.Application.Common.Constants;
using COFIDE.WFF.Domain.Common.Tiempo;
namespace COFIDE.WFF.Application.Features.Solicitudes.EventHandlers;

public class EnviarCorreoPorAprobarHandler(
        ISolicitudQueryRepository solicitudQueryRepository,
        ISolicitudPlantillaRepository solicitudPlantillaQueryRepository,
        ICorreoSenderPort correoSenderPort,
        IFormatoNotificacionRepository formatoNotificacionRepository,
        IHtmlTemplateRenderer htmlTemplateRenderer,
        IUrlProvider urlProvider,
        ILogger<EnviarCorreoPorAprobarHandler> logger
    ) : INotificationHandler<DomainEventNotification<SolicitudPorAprobarFirmantesDomainEvent>>
{
    private readonly ISolicitudQueryRepository _solicitudQueryRepository = solicitudQueryRepository;
    private readonly ISolicitudPlantillaRepository _solicitudPlantillaQueryRepository = solicitudPlantillaQueryRepository;
    private readonly ICorreoSenderPort _correoSenderPort = correoSenderPort;
    private readonly IFormatoNotificacionRepository _formatoNotificacionRepository = formatoNotificacionRepository;
    private readonly IHtmlTemplateRenderer _htmlTemplateRenderer = htmlTemplateRenderer;
    private readonly IUrlProvider _urlProvider = urlProvider;
    private readonly ILogger<EnviarCorreoPorAprobarHandler> _logger = logger;

    public async Task Handle(DomainEventNotification<SolicitudPorAprobarFirmantesDomainEvent> notification, CancellationToken cancellationToken)
    {
        string asunto = string.Empty;
        string cuerpo = string.Empty;

        try
        {
            var baseFrontUrl = _urlProvider.GetWorkflowBaseFrontUrl();
            var domainEvent = notification.DomainEvent;

            var templateCorreoNoUrgente = await _formatoNotificacionRepository
                .ObtenerPorCodigo(CodigosPlantillaCorreo.SolicitudPorAprobar, cancellationToken)
                ?? throw new InvalidOperationException("Template SOLICITUD_POR_APROBAR no configurado.");

            var solicitud = await _solicitudQueryRepository
                .ObtenerParaNotificaciones(domainEvent.IdSolicitud, cancellationToken)
                ?? throw new InvalidOperationException("Solicitud no encontrada para notificaciones.");

            if (!solicitud.EsUrgente)
            {
                var etapaActual = await _solicitudPlantillaQueryRepository
                    .ObtenerEtapaActual(domainEvent.IdSolicitud, 0, cancellationToken)
                    ?? throw new InvalidOperationException("No se encontró etapa actual.");

                var tipoEtapa = etapaActual.ValorTipoEtapa;

                var listaUsuarios = etapaActual.Firmantes
                .Where(f => f.Notificacion.HasValue && f.Notificacion == true)
                .Select(f => (f.Correo, f.NombreUsuarioAsignado ?? string.Empty))
                .ToList();

                asunto = _htmlTemplateRenderer.Render(
                    templateCorreoNoUrgente.AsuntoCorreo ?? string.Empty, solicitud);

                if (tipoEtapa == "SECUENCIAL")
                {
                    foreach (var item in listaUsuarios)
                    {
                        var modeloCorreoCuerpo = new
                        {
                            ParaAprobadorOAprobadores = "Estimado/a " + item.Item2,
                            etapaActual.NombreEtapa,
                            solicitud.TipoSolicitud,
                            solicitud.CodigoSolicitud,
                            solicitud.NombreSolicitante,
                            solicitud.SubTipoSolicitud,
                            solicitud.TipoOperacion,
                            solicitud.AsuntoSolicitud,
                            solicitud.FechaCreacion,
                            Anio = HoraSistema.Ahora.Year.ToString(),
                            LinkSolicitud = $"{baseFrontUrl}/mis_solicitudes/detalle/editar?id={domainEvent.IdSolicitud}",
                            LinkAprobarSolicitud = $"{baseFrontUrl}/bandejas/detalle-solicitud?id={domainEvent.IdSolicitud}"
                        };

                        cuerpo = _htmlTemplateRenderer.Render(
                            templateCorreoNoUrgente.CuerpoMensaje ?? string.Empty, modeloCorreoCuerpo);

                        await _correoSenderPort.EnviarCorreoAsync(
                            asunto, cuerpo,
                            item.Item1,
                            templateCorreoNoUrgente.CorreoRemitente ?? string.Empty,
                            cancellationToken);
                    }
                }

                if (tipoEtapa == "PARALELO")
                {
                    var modeloCorreoCuerpo = new
                    {
                        ParaAprobadorOAprobadores = "Estimado/a ",
                        etapaActual.NombreEtapa,
                        solicitud.TipoSolicitud,
                        solicitud.CodigoSolicitud,
                        solicitud.NombreSolicitante,
                        solicitud.SubTipoSolicitud,
                        solicitud.TipoOperacion,
                        solicitud.AsuntoSolicitud,
                        solicitud.FechaCreacion,
                        Anio = HoraSistema.Ahora.Year.ToString(),
                        LinkSolicitud = $"{baseFrontUrl}/mis-solicitudes",
                        LinkAprobarSolicitud = $"{baseFrontUrl}/bandejas/detalle-solicitud?id={domainEvent.IdSolicitud}"
                    };

                    cuerpo = _htmlTemplateRenderer.Render(
                        templateCorreoNoUrgente.CuerpoMensaje ?? string.Empty, modeloCorreoCuerpo);

                    await _correoSenderPort.EnviarCorreoAsync(
                        asunto, cuerpo,
                        string.Join(";", listaUsuarios.Select(x => x.Item1)),
                        templateCorreoNoUrgente.CorreoRemitente ?? string.Empty,
                        cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al notificar un correo por aprobar");
        }
    }
}
