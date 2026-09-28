using COFIDE.WFF.Application.Common.Events;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Events;
using ValueObject = COFIDE.WFF.Domain.ValueObjects;
using MediatR;
namespace COFIDE.WFF.Application.Features.Firmas.EventHandlers;

public class DocumentoFirmadoHubEventHandler(
    INotificacionTiempoRealPort notificacionTiempoRealPort
    ) : INotificationHandler<DomainEventNotification<DocumentoFirmadoDomainEvent>>
{
    private readonly INotificacionTiempoRealPort _notificacionTiempoRealPort = notificacionTiempoRealPort;


    public async Task Handle(DomainEventNotification<DocumentoFirmadoDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        await _notificacionTiempoRealPort.NotificarFirmaDocumentoAsync(
                domainEvent.IdSolicitud, 
                domainEvent.IdSolicitudDocumento,
                ValueObject.EstadoSolicitud.EnCurso.Codigo.ToString(),
                domainEvent.firmado,
                domainEvent.CodigoUsuario,
                domainEvent.Mensaje,
                cancellationToken);
    }
}