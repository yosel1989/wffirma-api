using COFIDE.WFF.Application.Common.Events;
using COFIDE.WFF.Application.Features.Plantillas.Queries.ObtenerPorId;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Events;
using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
using COFIDE.WFF.Application.Common.Constants;
using COFIDE.WFF.Domain.Common.Tiempo;
namespace COFIDE.WFF.Application.Features.Solicitudes.EventHandlers;

public class EnviarCorreoUrgentePorAprobarHandler(
        ISolicitudQueryRepository solicitudQueryRepository,
        ISolicitudPlantillaRepository solicitudPlantillaQueryRepository,
        ICorreoSenderPort correoSenderPort,
        IFormatoNotificacionRepository formatoNotificacionRepository,
        IHtmlTemplateRenderer htmlTemplateRenderer,
        IUrlProvider urlProvider,
        ILogger<EnviarCorreoUrgentePorAprobarHandler> logger
    ) : INotificationHandler<DomainEventNotification<SolicitudCreadaDomainEvent>>
{
    private readonly ISolicitudQueryRepository _solicitudQueryRepository = solicitudQueryRepository;
    private readonly ISolicitudPlantillaRepository _solicitudPlantillaQueryRepository = solicitudPlantillaQueryRepository;
    private readonly ICorreoSenderPort _correoSenderPort = correoSenderPort;
    private readonly IFormatoNotificacionRepository _formatoNotificacionRepository = formatoNotificacionRepository;
    private readonly IHtmlTemplateRenderer _htmlTemplateRenderer = htmlTemplateRenderer;
    private readonly IUrlProvider _urlProvider = urlProvider;
    private readonly ILogger<EnviarCorreoUrgentePorAprobarHandler> _logger = logger;

    public async Task Handle(
        DomainEventNotification<SolicitudCreadaDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        try
        {
            var baseFrontUrl = _urlProvider.GetWorkflowBaseFrontUrl().TrimEnd('/');
            var domainEvent = notification.DomainEvent;

            var templateCorreoUrgente = await _formatoNotificacionRepository
                .ObtenerPorCodigo(CodigosPlantillaCorreo.SolicitudPorAprobarUrgente, cancellationToken)
                ?? throw new InvalidOperationException("Template SOLICITUD_POR_APROBAR_URGENTE no configurado.");

            var solicitud = await _solicitudQueryRepository
                .ObtenerParaNotificaciones(domainEvent.IdSolicitud, cancellationToken)
                ?? throw new InvalidOperationException("Solicitud no encontrada para notificaciones.");

            if (!solicitud.EsUrgente)
                return;

            var etapaActual = await _solicitudPlantillaQueryRepository
                .ObtenerEtapaActual(domainEvent.IdSolicitud, 0, cancellationToken)
                ?? throw new InvalidOperationException("No se encontró etapa actual.");

            var tipoEtapa = etapaActual.ValorTipoEtapa;
            var listaUsuarios = etapaActual.Firmantes
                .Where(f => f.Notificacion.HasValue && f.Notificacion == true)
                .Select(f => (f.Correo, f.NombreUsuarioAsignado ?? string.Empty))
                .ToList();

            var asunto = _htmlTemplateRenderer.Render(
                templateCorreoUrgente.AsuntoCorreo
                    ?? throw new InvalidOperationException("AsuntoCorreo no configurado."),
                solicitud);

            await EnviarCorreosPorTipoEtapaAsync(
                tipoEtapa, listaUsuarios, etapaActual, solicitud,
                templateCorreoUrgente, domainEvent.IdSolicitud,
                baseFrontUrl, asunto, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al notificar un correo urgente por aprobar");
        }
    }

    private async Task EnviarCorreosPorTipoEtapaAsync(
        string tipoEtapa,
        List<(string Correo, string Nombre)> listaUsuarios,
        PlantillaEtapaDto etapaActual,
        FormatoNotificacionSolicitud solicitud,
        FormatoNotificacion template,
        long idSolicitud,
        string baseUrl,
        string asunto,
        CancellationToken cancellationToken)
    {
        if (tipoEtapa == "SECUENCIAL")
        {
            await EnviarCorreoSecuencialAsync(
                listaUsuarios, etapaActual, solicitud,
                template, idSolicitud, baseUrl, asunto, cancellationToken);
            return;
        }

        if (tipoEtapa == "PARALELO")
        {
            await EnviarCorreoParaleloAsync(
                listaUsuarios, etapaActual, solicitud,
                template, idSolicitud, baseUrl, asunto, cancellationToken);
        }
    }

    private async Task EnviarCorreoSecuencialAsync(
        List<(string Correo, string Nombre)> listaUsuarios,
        PlantillaEtapaDto etapaActual,
        FormatoNotificacionSolicitud solicitud,
        FormatoNotificacion template,
        long idSolicitud,
        string baseUrl,
        string asunto,
        CancellationToken cancellationToken)
    {
        foreach (var item in listaUsuarios)
        {
            var modelo = ConstruirModelo(
                "Estimado/a " + item.Nombre,
                etapaActual, solicitud, idSolicitud, baseUrl);

            var cuerpo = _htmlTemplateRenderer.Render(
                template.CuerpoMensaje
                    ?? throw new InvalidOperationException("CuerpoMensaje no configurado."),
                modelo);

            await _correoSenderPort.EnviarCorreoAsync(
                asunto, cuerpo, item.Correo, template.CorreoRemitente ?? string.Empty, cancellationToken);
        }
    }

    private async Task EnviarCorreoParaleloAsync(
        List<(string Correo, string Nombre)> listaUsuarios,
        PlantillaEtapaDto etapaActual,
        FormatoNotificacionSolicitud solicitud,
        FormatoNotificacion template,
        long idSolicitud,
        string baseUrl,
        string asunto,
        CancellationToken cancellationToken)
    {
        var modelo = ConstruirModelo(
            "Estimados aprobadores",
            etapaActual, solicitud, idSolicitud, baseUrl);

        var cuerpo = _htmlTemplateRenderer.Render(
            template.CuerpoMensaje
                ?? throw new InvalidOperationException("CuerpoMensaje no configurado."),
            modelo);

        var destinatarios = string.Join(";", listaUsuarios.Select(x => x.Correo));

        await _correoSenderPort.EnviarCorreoAsync(
            asunto, cuerpo, destinatarios, template.CorreoRemitente ?? string.Empty, cancellationToken);
    }

    private static object ConstruirModelo(
        string paraAprobador,
        PlantillaEtapaDto etapaActual,
        FormatoNotificacionSolicitud solicitud,
        long idSolicitud,
        string baseUrl)
    {
        return new
        {
            ParaAprobadorOAprobadores = paraAprobador,
            etapaActual.NombreEtapa,
            FechaLimiteCadena = solicitud.FechaLimite?.ToString("dd/MM/yyyy HH:mm"),
            CantidadAprobadoresFaltantes = etapaActual.Firmantes.Count.ToString(),
            solicitud.TipoSolicitud,
            solicitud.CodigoSolicitud,
            solicitud.NombreSolicitante,
            solicitud.SubTipoSolicitud,
            solicitud.TipoOperacion,
            solicitud.AsuntoSolicitud,
            solicitud.FechaCreacion,
            Anio = HoraSistema.Ahora.Year.ToString(),
            LinkSolicitud = $"{baseUrl}/mis_solicitudes/detalle/visualizar?id={idSolicitud}",
            LinkAprobarSolicitud = $"{baseUrl}/bandejas/detalle-solicitud?id={idSolicitud}"
        };
    }
}
