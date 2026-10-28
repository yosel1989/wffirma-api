namespace COFIDE.WFF.Api.Request.AccesosEspeciales
{
    /// <summary>
    /// Representa una solicitud para cambiar el estado de un acceso especial.
    /// </summary>
    public sealed class CambiarEstadoAccesoEspecialRequest
    {
        /// <summary>
        /// Identificador único del acceso especial.
        /// </summary>
        public long IdAccesoEspecial { get; set; }

        /// <summary>
        /// Nuevo estado que se asignará al acceso especial.
        /// </summary>
        public string Estado { get; set; } = string.Empty;
    }
}
