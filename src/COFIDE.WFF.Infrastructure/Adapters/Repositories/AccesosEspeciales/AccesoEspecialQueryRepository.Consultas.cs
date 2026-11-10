using COFIDE.WFF.Domain.Aggregates.AccesoEspecialAggregate.Vo;
using Microsoft.EntityFrameworkCore;

namespace COFIDE.WFF.Infrastructure.Adapters.Repositories.AccesosEspeciales
{
    public partial class AccesoEspecialQueryRepository
    {

        /// <summary>
        /// Verifica si ya existe un acceso especial para el usuario y el área indicados.
        /// </summary>
        /// <param name="idUsuario">Identificador único del usuario al que se le asignó el acceso especial.</param>
        /// <param name="idArea">Identificador único del área (gerencia y/o departamento) asociada al acceso especial.</param>
        /// <param name="cancellationToken">Token para manejar la cancelación de la operación.</param>
        /// <returns>
        /// Una tarea que representa la operación asincrónica. El resultado es <c>true</c> si el acceso
        /// especial ya existe; de lo contrario, <c>false</c>.
        /// </returns>
        public async Task<bool> ExistAsync(long idUsuario, long idArea, CancellationToken cancellationToken)
        {
            var total = await _context.AccesosEspeciales
                .IgnoreQueryFilters()
                .CountAsync(d => d.IdUsuario == idUsuario && d.IdArea == idArea && d.CodigoEstado != EstadoCodigo.Eliminado, cancellationToken);

            return total > 0;
        }

    }
}
