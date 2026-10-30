namespace COFIDE.WFF.Api.Request.AccesosEspeciales
{
    /// <summary>
    /// Representa una solicitud para cambiar el estado de un acceso especial.
    /// </summary>
    public sealed class CambiarEstadoAccesoEspecialRequest
    {
        /// <summary>
        /// Nuevo estado que se asignará al acceso especial.
        /// </summary>
        public bool Estado { get; set; }
    }
}
