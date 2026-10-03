using COFIDE.WFF.Domain.Common.Abstractions;

namespace COFIDE.WFF.Domain.Common.Events;
public abstract record DomainEvent : IDomainEvent
{
    // init: permite conservar la fecha original si el evento se serializa (por ejemplo, al publicarlo en una cola).
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}
