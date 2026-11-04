using COFIDE.WFF.Domain.Aggregates.AccesoEspecialAggregate.Vo;
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
        /// Identificador del área (gerencia y/o departamento) asociada al acceso especial.
        /// </summary>
        public long IdArea { get; private set; }

        /// <summary>
        /// Código del estado.
        /// </summary>
        public EstadoCodigo CodigoEstado { get; private set; } = EstadoCodigo.Activo;

        /// <summary>
        /// Estado eliminado del acceso especial, de solo lectura.
        /// </summary>
        public bool Eliminado => CodigoEstado.Equals(EstadoCodigo.Eliminado);

        #endregion

        #region Relaciones

        /// <summary>
        /// Relación con la entidad Usuario.
        /// </summary>
        public Usuario Usuario { get; private set; } = null!;

        /// <summary>
        /// Relación con la entidad Area que representa el área (gerencia y/o departamento).
        /// </summary>
        public Area Area { get; private set; } = null!;


        #endregion

        /// <summary>
        /// Constructor privado para cumplir con el patrón de diseño de creación controlada.
        /// </summary>
        private AccesoEspecial() { }

        #region Helpers
        // Métodos auxiliares pueden ser agregados aquí en el futuro.

        public new void Activar() => CodigoEstado = EstadoCodigo.Activo;
        public new void Desactivar() => CodigoEstado = EstadoCodigo.Inactivo;
        public void Eliminar() => CodigoEstado = EstadoCodigo.Eliminado;

        #endregion

        #region Crear

        /// <summary>
        /// Crea una nueva instancia de <see cref="AccesoEspecial"/> con los datos proporcionados.
        /// </summary>
        /// <param name="id">Identificador único del acceso especial.</param>
        /// <param name="idUsuario">Identificador del usuario asociado.</param>
        /// <param name="idArea">Identificador del área asociada (gerencio y/o departamento).</param>
        /// <returns>Una nueva instancia de <see cref="AccesoEspecial"/>.</returns>
        public static AccesoEspecial Crear(
            long id,
            long idUsuario,
            long idArea)
        {
            var accesoEspecial = new AccesoEspecial
            {
                Id = id,
                IdUsuario = idUsuario,
                IdArea = idArea,
                Estado = true
            };

            return accesoEspecial;
        }

        #endregion
    }
}
