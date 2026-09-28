using COFIDE.WFF.Domain.Common.Abstractions;
using COFIDE.WFF.Domain.Common.Events;
namespace COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Events;

public record SolicitudEventosBatchDomainEvent(
  List<SolicitudEventoRegistradoDomainEvent> Eventos
) : DomainEvent, IPostCommitDomainEvent;
