using COFIDE.WFF.Application.Common.Events;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Events;
using MediatR;
using Microsoft.Extensions.Logging;
namespace COFIDE.WFF.Application.Features.Solicitudes.EventHandlers;

public class RegistrarEventosBatchHandler(
        ISolicitudEventoRepository solicitudEventoRepository,
        IUnitOfWork unitOfWork,
        ILogger<RegistrarEventosBatchHandler> logger
    ) : INotificationHandler<DomainEventNotification<SolicitudEventosBatchDomainEvent>>
{

    private readonly ISolicitudEventoRepository _solicitudEventoRepository = solicitudEventoRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<RegistrarEventosBatchHandler> _logger = logger;

    public async Task Handle(DomainEventNotification<SolicitudEventosBatchDomainEvent> notification, CancellationToken cancellationToken)
    {
        try
        {
            var eventos = notification.DomainEvent.Eventos;

            await _unitOfWork.BeginTransactionAsync();


            var cantidadEventos = eventos.Count;
            var rangoEventos = await _solicitudEventoRepository.ObtenerBloqueSiguienteIdEventoAsync(cantidadEventos, cancellationToken);
            var indexEventos = 0;
            Func<long> generarIdEvento = () => rangoEventos[indexEventos++];


            foreach (var e in eventos)
            {
                var entity = SolicitudEvento.Crear(
                    e.IdSolicitud,
                    generarIdEvento(),
                    e.IdTipoEvento,
                    e.IdUsuarioOrigen,
                    e.IdUsuarioDestino,
                    e.IdPlantillaEtapa,
                    e.AreaUsuario,
                    e.IdSolicitudSustento,
                    DateTime.Now
                );

                await _solicitudEventoRepository.AddAsync(entity);
            }

            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error al registrar solicitud evento");
        }
    }
}
