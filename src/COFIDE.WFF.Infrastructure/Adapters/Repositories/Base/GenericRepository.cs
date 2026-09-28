using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Domain.Common.Abstractions;
using COFIDE.WFF.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace COFIDE.WFF.Infrastructure.Adapters.Repositories.Base;

/// <summary>
/// Repositorio genérico base que proporciona operaciones comunes de acceso a datos (CRUD y paginación).
/// Aplica patrón Repository y soporta auditoría básica.
/// </summary>
/// <typeparam name="T">Tipo de entidad (debe implementar IAuditableEntity).</typeparam>
/// <typeparam name="TId">Tipo de identificador de la entidad.</typeparam>
public class GenericRepository<T, TId> : IGenericRepository<T, TId> where T : class, IAuditableEntity<TId> where TId : struct
{
    protected readonly WorkflowFirmaDbContext _context;
    private readonly DbSet<T> _dbSet;

    /// <summary>
    /// Inicializa una nueva instancia del repositorio genérico.
    /// </summary>
    public GenericRepository(WorkflowFirmaDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = context.Set<T>();
    }

    /// <summary>
    /// Obtiene una entidad por su ID si no está eliminada.
    /// </summary>
    /// <param name="id">ID de la entidad.</param>
    /// <returns>Entidad encontrada o null.</returns>
    public virtual async Task<T?> GetByIdAsync(TId id, bool tracking = false)
    {
        IQueryable<T> query = _dbSet;

        if (!tracking)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(e => e.Id.Equals(id) && e.Estado);
    }

    /// <summary>
    /// Obtiene todas las entidades no eliminadas.
    /// </summary>
    /// <returns>Lista de entidades.</returns>
    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.AsNoTracking().Where(e => e.Estado).ToListAsync();
    }

    /// <summary>
    /// Agrega una nueva entidad al contexto.
    /// </summary>
    /// <param name="entity">Entidad a agregar.</param>
    public virtual async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    /// <summary>
    /// Actualiza una entidad en el contexto.
    /// </summary>
    /// <param name="entity">Entidad a actualizar.</param>
    public virtual Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken)
    {
        await _context.Set<T>().AddRangeAsync(entities, cancellationToken);
    }

    public virtual Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken)
    {
        _dbSet.UpdateRange(entities);
        return Task.CompletedTask;
    }
}
