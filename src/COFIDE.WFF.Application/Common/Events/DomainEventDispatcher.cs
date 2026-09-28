using COFIDE.WFF.Domain.Common.Abstractions;
using MediatR;
namespace COFIDE.WFF.Application.Common.Events;
public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IMediator _mediator;

    public DomainEventDispatcher(IMediator mediator)
    {
        _mediator = mediator;
    }

    public Task DispatchPreCommitAsync(
       IEnumerable<IDomainEvent> domainEvents,
       CancellationToken cancellationToken)
    {
        var events = domainEvents
            .Where(x => x is IPreCommitDomainEvent);

        return Publish(events, cancellationToken);
    }

    public Task DispatchPostCommitAsync(
       IEnumerable<IDomainEvent> domainEvents,
       CancellationToken cancellationToken)
    {
        var events = domainEvents
            .Where(x => x is IPostCommitDomainEvent);

        return Publish(events, cancellationToken);
    }

    private async Task Publish(
       IEnumerable<IDomainEvent> domainEvents,
       CancellationToken cancellationToken)
    {
        foreach (var domainEvent in domainEvents)
        {
            var notificationType = typeof(DomainEventNotification<>)
                .MakeGenericType(domainEvent.GetType());

            var notification = (INotification)Activator
                .CreateInstance(notificationType, domainEvent)!;

            await _mediator.Publish(notification, cancellationToken);
        }
    }

    public async Task DispatchAsync(
        IEnumerable<IDomainEvent> domainEvents,
        CancellationToken cancellationToken)
    {
        foreach (var domainEvent in domainEvents)
        {
            var notificationType = typeof(DomainEventNotification<>)
                .MakeGenericType(domainEvent.GetType());

            var notification = (INotification)Activator
                .CreateInstance(notificationType, domainEvent)!;

            await _mediator.Publish(notification, cancellationToken);
        }
    }
}
