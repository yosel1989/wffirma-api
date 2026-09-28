using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Events;
namespace COFIDE.WFF.Application.Features.Solicitudes.Services;

public interface ISolicitudEventoFactory
{
    List<SolicitudEventoRegistradoDomainEvent> CrearEventos(
    long idSolicitud,
    SolicitudFlujoDiff diff);

    List<SolicitudEventoRegistradoDomainEvent> CrearEventoSustento(
    long idSolicitud,
    long idSustento);
}
