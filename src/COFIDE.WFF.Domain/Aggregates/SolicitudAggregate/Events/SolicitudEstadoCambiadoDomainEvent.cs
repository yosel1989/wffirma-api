using COFIDE.WFF.Domain.Common.Abstractions;
using COFIDE.WFF.Domain.Common.Events;
namespace COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Events;

public sealed record SolicitudEstadoCambiadoDomainEvent(
 long IdSolicitud,
 long IdEstadoAnterior,
 long IdUsuarioEstado,
 string MotivoCambio
) : DomainEvent, IPostCommitDomainEvent;
