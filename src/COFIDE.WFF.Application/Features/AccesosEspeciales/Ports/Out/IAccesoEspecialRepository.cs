using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.AccesoEspecialAggregate;

namespace COFIDE.WFF.Application.Features.AccesosEspeciales.Ports.Out
{
    /// <summary>
    /// Interfaz para el repositorio de AccesoEspecial que extiende las operaciones genéricas.
    /// </summary>
    public interface IAccesoEspecialRepository : IGenericRepository<AccesoEspecial, long>
    {
        /// <summary>
        /// Registra un acceso especial de forma asíncrona.
        /// </summary>
        /// <param name="accesoEspecial">Entidad de acceso especial a guardar.</param>
        void RegistrarAccesoEspecialAsync(AccesoEspecial accesoEspecial);

        /// <summary>
        /// Elimina físicamente un acceso especial.
        /// </summary>
        /// <param name="accesoEspecial">Entidad de acceso especial a eliminar.</param>
        void Delete(AccesoEspecial accesoEspecial);

        /// <summary>
        /// Obtiene un acceso especial por su identificador, sin filtrar por estado
        /// (incluye registros activos e inactivos).
        /// </summary>
        /// <param name="id">Identificador único del acceso especial.</param>
        /// <param name="cancellationToken">Token para manejar la cancelación de la operación.</param>
        /// <returns>El acceso especial encontrado, o <c>null</c> si no existe.</returns>
        Task<AccesoEspecial?> GetByIdAnyStatusAsync(long id, bool tracking);
    }
}