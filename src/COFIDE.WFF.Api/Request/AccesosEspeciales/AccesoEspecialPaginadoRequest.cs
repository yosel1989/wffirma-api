using COFIDE.WFF.Api.Request.Comun;

namespace COFIDE.WFF.Api.Request.AccesosEspeciales
{
    /// <summary>
    /// Representa una solicitud de paginación para acceder a datos de accesos especiales.
    /// Hereda de <see cref="PaginadoRequest"/> para incluir parámetros comunes de paginación y ordenamiento.
    /// </summary>
    public sealed class AccesoEspecialPaginadoRequest : PaginadoRequest
    {
        /// <summary>
        /// Identificador de la gerencia asociado al acceso especial.
        /// </summary>
        public long? IdGerencia { get; set; }

        /// <summary>
        /// Identificador del departamento asociado al acceso especial.
        /// </summary>
        public long? IdDepartamento { get; set; }

        /// <summary>
        /// Identificador del usuario asociado al acceso especial.
        /// </summary>
        public long? IdUsuario { get; set; }
    }
}
