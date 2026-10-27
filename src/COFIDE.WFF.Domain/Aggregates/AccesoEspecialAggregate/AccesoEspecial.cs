using COFIDE.WFF.Domain.Common.Entities;
using COFIDE.WFF.Domain.Entities;

namespace COFIDE.WFF.Domain.Aggregates.AccesoEspecialAggregate
{
    /// <summary>
    /// Representa una entidad de acceso especial que define permisos específicos para un usuario en una gerencia y, opcionalmente, en un departamento.
    /// </summary>
    public class AccesoEspecial : AuditableEntity<long>
    {
        #region Propiedades

        /// <summary>
        /// Identificador del usuario asociado al acceso especial.
        /// </summary>
        public long IdUsuario { get; private set; }

        /// <summary>
        /// Identificador del área (gerencia) asociada al acceso especial.
        /// </summary>
        public long IdGerencia { get; private set; }

        /// <summary>
        /// Identificador del área (departamento) asociada al acceso especial (opcional).
        /// </summary>
        public long? IdDepartamento { get; private set; }

        #endregion

        #region Relaciones

        /// <summary>
        /// Relación con la entidad Usuario.
        /// </summary>
        public Usuario Usuario { get; private set; } = null!;

        /// <summary>
        /// Relación con la entidad Area que representa la gerencia.
        /// </summary>
        public Area Gerencia { get; private set; } = null!;

        /// <summary>
        /// Relación con la entidad Area que representa el departamento.
        /// </summary>
        public Area? Departamento { get; private set; } = null!;

        #endregion

        /// <summary>
        /// Constructor privado para cumplir con el patrón de diseño de creación controlada.
        /// </summary>
        private AccesoEspecial() { }

        #region Helpers
        // Métodos auxiliares pueden ser agregados aquí en el futuro.
        #endregion

        #region Creación

        /// <summary>
        /// Crea una nueva instancia de <see cref="AccesoEspecial"/> con los datos proporcionados.
        /// </summary>
        /// <param name="id">Identificador único del acceso especial.</param>
        /// <param name="idUsuario">Identificador del usuario asociado.</param>
        /// <param name="idGerencia">Identificador de la gerencia asociada.</param>
        /// <param name="idDepartamento">Identificador del departamento asociado (opcional).</param>
        /// <returns>Una nueva instancia de <see cref="AccesoEspecial"/>.</returns>
        public static AccesoEspecial Crear(
            long id,
            long idUsuario,
            long idGerencia,
            long? idDepartamento)
        {
            var accesoEspecial = new AccesoEspecial
            {
                Id = id,
                IdUsuario = idUsuario,
                IdGerencia = idGerencia,
                IdDepartamento = idDepartamento,
                Estado = true
            };

            return accesoEspecial;
        }

        #endregion
    }
}
