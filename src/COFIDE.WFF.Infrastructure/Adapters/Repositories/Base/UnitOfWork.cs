using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Domain.Common.Abstractions;
using COFIDE.WFF.Domain.Common.Entities;
using COFIDE.WFF.Infrastructure.Exceptions;
using COFIDE.WFF.Infrastructure.Persistence;
using COFIDE.WFF.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace COFIDE.WFF.Infrastructure.Adapters.Repositories.Base;

/// <summary>
/// Implementación del patrón Unit of Work utilizando Entity Framework Core.
/// Gestiona la persistencia y las transacciones de forma centralizada.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork, IAsyncDisposable
{
    private readonly WorkflowFirmaDbContext _context;
    private IDbContextTransaction? _transaction;

    /// <summary>
    /// Indica si existe una transacción activa en curso.
    /// </summary>
    public bool HasActiveTransaction => _transaction is not null;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="UnitOfWork"/>.
    /// </summary>
    /// <param name="context">Instancia del <see cref="WorkflowFirmaDbContext"/>.</param>
    public UnitOfWork(WorkflowFirmaDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }


    #region Persistence
    /// <summary>
    /// Guarda los cambios pendientes en la base de datos.
    /// </summary>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Cantidad de entidades afectadas.</returns>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new InfrastructureException(
                PersistenceErrors.ConcurrencyConflict,
                inner: ex);
        }
        catch (DbUpdateException ex)
        {
            throw new InfrastructureException(
                PersistenceErrors.SaveFailed,
                inner: ex);
        }
    }
    #endregion

    #region Transactions
    /// <summary>
    /// Inicia una transacción en la base de datos, si no hay una activa.
    /// </summary>
    public async Task BeginTransactionAsync()
    {
        if (_transaction is not null)
            return;

        _transaction = await _context.Database.BeginTransactionAsync();
    }

    /// <summary>
    /// Confirma la transacción actual y guarda los cambios.
    /// </summary>
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
            throw new InfrastructureException(PersistenceErrors.NoActiveTransaction);

        try
        {
            // Guarda cambios
            await _context.SaveChangesAsync(cancellationToken);
            // Commit
            await _transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    /// <summary>
    /// Revierte la transacción activa actual.
    /// </summary>
    public async Task RollbackTransactionAsync()
    {
        if (_transaction is null)
            return;

        await _transaction.RollbackAsync();
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    #endregion

    #region Domain Events
    /// <summary>
    /// Obtiene y limpia todos los Domain Events generados por las entidades.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> GetDomainEvents()
    {
        var entities = _context.ChangeTracker
            .Entries<BaseEntity<long>>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        var events = entities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        entities.ForEach(e => e.ClearDomainEvents());

        return events;
    }

    #endregion

    #region Dispose

    /// <summary>
    /// Libera los recursos usados por el contexto y la transacción.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        await _context.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #endregion


    public void MarcarComoNuevo<TEntity>(TEntity entity) where TEntity : class
    {
        _context.Add(entity);
    }

    public void VerEstados()
    {
        var entries = _context.ChangeTracker.Entries()
            .Select(e => new
            {
                Entity = e.Entity.GetType().Name,
                e.State,
                Keys = string.Join(", ",
                    e.Properties
                     .Where(p => p.Metadata.IsPrimaryKey())
                     .Select(p => $"{p.Metadata.Name}={p.CurrentValue}")
                )
            });

        foreach (var item in entries)
        {
            Console.WriteLine(
                $"Entity: {item.Entity} | State: {item.State} | Keys: {item.Keys}"
            );
        }
    }
}