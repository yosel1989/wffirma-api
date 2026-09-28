using COFIDE.WFF.Domain.Common.Abstractions;
using COFIDE.WFF.Domain.Common.Events;
namespace COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Events;

public sealed record SolicitudCreadaBorradorDomainEvent(
    long IdSolicitud, string? correoSolicitante
) : DomainEvent, IPostCommitDomainEvent;
