namespace COFIDE.WFF.Application.Features.AccesosEspeciales.DTOs
{
    /// <summary>
    /// Representa una solicitud para cambiar el estado (activo/inactivo) de un acceso especial.
    /// </summary>
    public class AccesoEspecialCambiarEstadoRequestDto
    {
        /// <summary>
        /// Identificador único del acceso especial cuyo estado se desea cambiar.
        /// </summary>
        public long IdAccesoEspecial { get; set; }

        /// <summary>
        /// Nuevo estado del acceso especial: <c>true</c> para activarlo y <c>false</c> para desactivarlo.
        /// </summary>
        public bool Estado { get; set; }
    }
}