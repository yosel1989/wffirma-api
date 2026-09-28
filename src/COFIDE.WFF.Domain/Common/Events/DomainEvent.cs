using COFIDE.WFF.Domain.Common.Abstractions;

namespace COFIDE.WFF.Domain.Common.Events;
public abstract record DomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
