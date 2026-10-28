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
        /// Identificador del área (departamento y/o gerencia) asociado al acceso especial.
        /// </summary>
        public long? IdArea{ get; set; }

        /// <summary>
        /// Identificador del usuario asociado al acceso especial.
        /// </summary>
        public long? IdUsuario { get; set; }
    }
}
