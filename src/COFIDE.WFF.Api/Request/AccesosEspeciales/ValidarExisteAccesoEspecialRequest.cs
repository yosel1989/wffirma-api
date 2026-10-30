using COFIDE.WFF.Api.Request.Comun;

namespace COFIDE.WFF.Api.Request.AccesosEspeciales
{
    /// <summary>
    /// Representa una solicitud de validación si existe un acceso especial.
    /// </summary>
    public sealed class ValidarExisteAccesoEspecialRequest
    {

        /// <summary>
        /// Identificador del usuario asociado al acceso especial.
        /// </summary>
        public long IdUsuario { get; set; }


        /// <summary>
        /// Identificador del área (departamento y/o gerencia) asociado al acceso especial.
        /// </summary>
        public long IdArea { get; set; }

    }
}
