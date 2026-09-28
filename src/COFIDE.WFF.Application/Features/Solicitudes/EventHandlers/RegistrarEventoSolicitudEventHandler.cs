using COFIDE.WFF.Application.Common.Events;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Events;
using MediatR;
using Microsoft.Extensions.Logging;
namespace COFIDE.WFF.Application.Features.Solicitudes.EventHandlers;

public class RegistrarEventoSolicitudEventHandler(
        ISolicitudEventoRepository solicitudEventoRepository,
        IUnitOfWork unitOfWork,
        ILogger<RegistrarEventoSolicitudEventHandler> logger
    ) : INotificationHandler<DomainEventNotification<SolicitudEventoRegistradoDomainEvent>>
{
    private readonly ISolicitudEventoRepository _solicitudEventoRepository = solicitudEventoRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<RegistrarEventoSolicitudEventHandler> _logger = logger;

    public async Task Handle(DomainEventNotification<SolicitudEventoRegistradoDomainEvent> notification, CancellationToken cancellationToken)
    {
        try
        {
            // obtenemos los campos
            var domainEvent = notification.DomainEvent;

            await _unitOfWork.BeginTransactionAsync();

            var id = await _solicitudEventoRepository.ObtenerSiguienteIdEventoAsync(cancellationToken);

            // Setear a la entidad
            var solicitudEvento = SolicitudEvento.Crear(
                   domainEvent.IdSolicitud,
                   id,
                   domainEvent.IdTipoEvento,
                   domainEvent.IdUsuarioOrigen,
                   domainEvent.IdUsuarioDestino,
                   domainEvent.IdPlantillaEtapa,
                   domainEvent.AreaUsuario,
                   domainEvent.IdSolicitudSustento,
                   DateTime.Now);

            // Registramos la solicitud evento
            await _solicitudEventoRepository.AddAsync(solicitudEvento);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error al registrar solicitud evento");
        }
    }

}
