using COFIDE.WFF.Application.Common.Events;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using COFIDE.WFF.Domain.Common.Tiempo;
namespace COFIDE.WFF.Application.Features.Solicitudes.EventHandlers;

public class RegistrarMovimientoSolicitudEventHandler(
        IGeneradorCorrelativoIdRepository generadorCorrelativoIdRepository,
        IMovimientoSolicitudRepository movimientoSolicitudRepository,
        IUnitOfWork unitOfWork,
        ILogger<RegistrarMovimientoSolicitudEventHandler> logger
    ) : INotificationHandler<DomainEventNotification<SolicitudEstadoCambiadoDomainEvent>>
{

    private readonly IGeneradorCorrelativoIdRepository _generadorCorrelativoIdRepository = generadorCorrelativoIdRepository;
    private readonly IMovimientoSolicitudRepository _movimientoSolicitudRepository = movimientoSolicitudRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<RegistrarMovimientoSolicitudEventHandler> _logger = logger;

    public async Task Handle(DomainEventNotification<SolicitudEstadoCambiadoDomainEvent> notification, CancellationToken cancellationToken)
    {
        try
        {

            // obtenemos los campos
            var domainEvent = notification.DomainEvent;

            await _unitOfWork.BeginTransactionAsync();

            var id = await _generadorCorrelativoIdRepository.ObtenerSiguienteIdMovimientoSolicitudAsync(cancellationToken);

            // Setear a la entidad
            var movimientoSolicitud = MovimientoSolicitud.Crear(
                   domainEvent.IdSolicitud,
                   id,
                   domainEvent.IdEstadoAnterior,
                   domainEvent.IdUsuarioEstado,
                   HoraSistema.Ahora,
                   domainEvent.MotivoCambio);

            // Registramos el movimiento de solicitud
            await _movimientoSolicitudRepository.AddAsync(movimientoSolicitud);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error al registrar movimiento solicitud");
        }
    }
}
