using COFIDE.WFF.Application.Common.Constants;
using COFIDE.WFF.Application.Common.Events;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Events;
using MediatR;
namespace COFIDE.WFF.Application.Features.Solicitudes.EventHandlers;

/// <summary>
/// Avisa en tiempo real a cada firmante en turno que la solicitud llegó a su bandeja.
/// El aviso de solicitud pendiente de firma es adicional al correo y no está asociado a un documento.
/// </summary>
public class NotificarSolicitudPorAprobarHubEventHandler(
        ISolicitudQueryRepository solicitudQueryRepository,
        INotificacionTiempoRealPort notificacionTiempoRealPort
    ) : INotificationHandler<DomainEventNotification<SolicitudPorAprobarFirmantesDomainEvent>>
{
    private readonly ISolicitudQueryRepository _solicitudQueryRepository = solicitudQueryRepository;
    private readonly INotificacionTiempoRealPort _notificacionTiempoRealPort = notificacionTiempoRealPort;

    /// <summary>
    /// Envía el aviso de solicitud pendiente de firma a cada firmante que está en turno.
    /// </summary>
    /// <param name="notification">Evento de solicitud por aprobar publicado después del commit.</param>
    /// <param name="cancellationToken">Token para cancelar las consultas y los envíos.</param>
    /// <returns>Una tarea que representa el procesamiento del evento.</returns>
    public async Task Handle(DomainEventNotification<SolicitudPorAprobarFirmantesDomainEvent> notification, CancellationToken cancellationToken)
    {
        var idSolicitud = notification.DomainEvent.IdSolicitud;

        var destinatarios = await _solicitudQueryRepository.ObtenerDestinatariosSolicitudPendienteFirmaAsync(idSolicitud, cancellationToken);

        foreach (var destinatario in destinatarios)
        {
            await _notificacionTiempoRealPort.NotificarSolicitudPendienteFirma(
                idSolicitud,
                destinatario.TotalSolicitudesPendientesFirma,
                new List<string> { destinatario.CodigoUsuario },
                MensajesNotificacion.SolicitudPorAprobar,
                cancellationToken);
        }
    }
}
