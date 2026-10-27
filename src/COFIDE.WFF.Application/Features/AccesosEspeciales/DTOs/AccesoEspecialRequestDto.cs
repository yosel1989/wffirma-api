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
        /// Identificador único del área (gerencia) asociada al acceso especial.
        /// </summary>
        public long IdGerencia { get; set; }

        /// <summary>
        /// Identificador único del área (departamento) asociado al acceso especial (opcional).
        /// </summary>
        public long? IdDepartamento { get; set; }
    }

}
