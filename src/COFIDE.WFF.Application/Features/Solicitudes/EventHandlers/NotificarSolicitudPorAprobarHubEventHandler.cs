using COFIDE.WFF.Application.Common.Constants;
using COFIDE.WFF.Application.Common.Events;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Events;
using MediatR;
using ValueObject = COFIDE.WFF.Domain.ValueObjects;
namespace COFIDE.WFF.Application.Features.Solicitudes.EventHandlers;

/// <summary>
/// Avisa en tiempo real a cada firmante en turno que la solicitud llego a su bandeja, reutilizando el envio
/// existente del NotificacionesHub (firma-documento). El tipo de aviso se identifica por el campo "mensaje"
/// (<see cref="CodigosNotificacionTiempoReal.SolicitudPorAprobar"/>), sin documento asociado (id 0).
/// Es adicional al correo; el adaptador del hub no propaga errores para no afectar la aprobacion ni el correo.
/// </summary>
public class NotificarSolicitudPorAprobarHubEventHandler(
        ISolicitudQueryRepository solicitudQueryRepository,
        INotificacionTiempoRealPort notificacionTiempoRealPort
    ) : INotificationHandler<DomainEventNotification<SolicitudPorAprobarFirmantesDomainEvent>>
{
    private readonly ISolicitudQueryRepository _solicitudQueryRepository = solicitudQueryRepository;
    private const long SinDocumento = 0;

    private readonly INotificacionTiempoRealPort _notificacionTiempoRealPort = notificacionTiempoRealPort;

    /// <summary>
    /// Envía el aviso por el mecanismo existente del hub a cada firmante que está en turno.
    /// </summary>
    /// <param name="notification">Evento de solicitud por aprobar publicado después del commit.</param>
    /// <param name="cancellationToken">Token para cancelar las consultas y los envíos.</param>
    /// <returns>Una tarea que representa el procesamiento del evento.</returns>
    public async Task Handle(DomainEventNotification<SolicitudPorAprobarFirmantesDomainEvent> notification, CancellationToken cancellationToken)
    {
        var idSolicitud = notification.DomainEvent.IdSolicitud;

        var codigosUsuario = await _solicitudQueryRepository.ObtenerCodigosUsuarioEnTurnoAsync(idSolicitud, cancellationToken);

        foreach (var codigoUsuario in codigosUsuario)
        {
            await _notificacionTiempoRealPort.NotificarFirmaDocumentoAsync(
                idSolicitud,
                SinDocumento,
                ValueObject.EstadoSolicitud.EnCurso.Codigo.ToString(),
                firmado: false,
                codigoUsuario,
                CodigosNotificacionTiempoReal.SolicitudPorAprobar,
                cancellationToken);
        }
    }
}
