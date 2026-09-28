using COFIDE.WFF.Domain.Common.Abstractions;
using MediatR;
namespace COFIDE.WFF.Application.Common.Events;

public sealed class DomainEventNotification<TDomainEvent> : INotification where TDomainEvent : IDomainEvent
{
    public TDomainEvent DomainEvent { get; }

    public DomainEventNotification(TDomainEvent domainEvent)
    {
        DomainEvent = domainEvent;
    }
}
