using COFIDE.WFF.Domain.Common.Abstractions;
using COFIDE.WFF.Domain.Common.Events;
namespace COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Events;

public sealed record SolicitudCreadaBorradorEnviadaEvent(long IdSolicitud, long IdEstadoSolicitud, long IdUsuarioEstado) : DomainEvent, IPostCommitDomainEvent;