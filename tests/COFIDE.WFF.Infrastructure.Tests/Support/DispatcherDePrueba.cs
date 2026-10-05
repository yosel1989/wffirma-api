using System.Collections.Concurrent;
using COFIDE.WFF.Application.Common.Events;
using COFIDE.WFF.Domain.Common.Abstractions;

namespace COFIDE.WFF.Infrastructure.Tests.Support;

/// <summary>
/// Doble de IDomainEventDispatcher: registra los eventos publicados y puede fallar
/// las primeras N veces para simular un servicio caído.
/// </summary>
internal sealed class DispatcherDePrueba(int fallosIniciales = 0, Func<IDomainEvent, bool>? fallaPara = null) : IDomainEventDispatcher
{
    private int _fallosRestantes = fallosIniciales;
    private readonly ConcurrentQueue<IDomainEvent> _publicados = new();

    public IReadOnlyCollection<IDomainEvent> Publicados => _publicados.ToArray();
    public int Intentos { get; private set; }

    public Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken)
    {
        foreach (var evento in domainEvents)
        {
            Intentos++;
            if (fallaPara?.Invoke(evento) == true || Interlocked.Decrement(ref _fallosRestantes) >= 0)
                throw new InvalidOperationException("Servicio no disponible (simulado)");

            _publicados.Enqueue(evento);
        }
        return Task.CompletedTask;
    }

    public Task DispatchPreCommitAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public Task DispatchPostCommitAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken) =>
        DispatchAsync(domainEvents, cancellationToken);
}
