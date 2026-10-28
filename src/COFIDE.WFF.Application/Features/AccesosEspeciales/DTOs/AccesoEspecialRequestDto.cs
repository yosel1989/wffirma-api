namespace COFIDE.WFF.Application.Features.AccesosEspeciales.DTOs
{
    /// <summary>
    /// Representa una solicitud para crear un acceso especial.
    /// </summary>
    public class AccesoEspecialRequestDto
    {
        /// <summary>
        /// Identificador único del usuario al que se le asignará el acceso especial.
        /// </summary>
        public long IdUsuario { get; set; }

        /// <summary>
        /// Identificador único del área (gerencia y/o departamento) asociada al acceso especial.
        /// </summary>
        public long IdArea { get; set; }

    }

}
