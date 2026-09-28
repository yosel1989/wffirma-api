using COFIDE.WFF.Domain.Common.Abstractions;
namespace COFIDE.WFF.Application.Common.Events;

public interface IDomainEventDispatcher
{
    Task DispatchPreCommitAsync(
       IEnumerable<IDomainEvent> domainEvents,
       CancellationToken cancellationToken);

    Task DispatchPostCommitAsync(
        IEnumerable<IDomainEvent> domainEvents,
        CancellationToken cancellationToken);

    Task DispatchAsync(
        IEnumerable<IDomainEvent> domainEvents,
        CancellationToken cancellationToken);
}
