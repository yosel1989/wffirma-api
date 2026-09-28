using COFIDE.WFF.Domain.Common.Abstractions;
namespace COFIDE.WFF.Application.Common.Ports.Out;


/// <summary>
/// Define una unidad de trabajo para coordinar transacciones y persistencia de datos.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Guarda todos los cambios pendientes en el contexto.
    /// </summary>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Inicia una nueva transacción de base de datos.
    /// </summary>
    Task BeginTransactionAsync();

    /// <summary>
    /// Confirma la transacción actual.
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Reversa la transacción actual.
    /// </summary>
    Task RollbackTransactionAsync();

    /// <summary>
    /// Obtiene y limpia todos los Domain Events generados por las entidades.
    /// </summary>
    IReadOnlyCollection<IDomainEvent> GetDomainEvents();

    /// <summary>
    /// Indica si hay una transacción activa.
    /// </summary>
    bool HasActiveTransaction { get; }
    /// <summary>
    /// Marca entidad como nueva
    /// </summary>
    void MarcarComoNuevo<TEntity>(TEntity entity) where TEntity : class;

    void VerEstados();
}
