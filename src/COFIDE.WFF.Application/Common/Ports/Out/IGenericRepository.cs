using COFIDE.WFF.Domain.Common.Abstractions;
namespace COFIDE.WFF.Application.Common.Ports.Out;

public interface IGenericRepository<TEntity, TId> where TEntity : class, IAuditableEntity<TId> where TId : struct
{
    /// <summary>
    /// Obtiene una entidad por su ID si no está eliminada.
    /// </summary>
    /// <param name="id">ID de la entidad.</param>
    /// <returns>Entidad encontrada o null.</returns>
    Task<TEntity?> GetByIdAsync(TId id, bool tracking = false);

    /// <summary>
    /// Obtiene todas las entidades no eliminadas.
    /// </summary>
    /// <returns>Lista de entidades.</returns>
    Task<IEnumerable<TEntity>> GetAllAsync();

    /// <summary>
    /// Agrega una nueva entidad al contexto.
    /// </summary>
    /// <param name="entity">Entidad a agregar.</param>
    Task AddAsync(TEntity entity);

    /// <summary>
    /// Actualiza una entidad en el contexto.
    /// </summary>
    /// <param name="entity">Entidad a actualizar.</param>
    Task UpdateAsync(TEntity entity);

    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken);

    Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken);
}
