namespace COFIDE.WFF.Application.Features.AccesosEspeciales.DTOs
{
    /// <summary>
    /// Representa un acceso especial asignado a un usuario en el sistema.
    /// </summary>
    public class AccesoEspecialPaginadoDto
    {
        /// <summary>
        /// Identificador único del acceso especial.
        /// </summary>
        public long IdAccesoEspecial { get; set; }

        /// <summary>
        /// Identificador único del usuario asociado al acceso especial.
        /// </summary>
        public long IdUsuario { get; set; }

        /// <summary>
        /// Nombre del usuario asociado al acceso especial.
        /// </summary>
        public string NombreUsuario { get; set; } = string.Empty;

        /// <summary>
        /// Nombre de la cuenta asociada al acceso especial.
        /// </summary>
        public string NombreCuenta { get; set; } = string.Empty;

        /// <summary>
        /// Identificador único del área (gerencia) asociada al acceso especial.
        /// </summary>
        public long IdGerencia { get; set; }

        /// <summary>
        /// Nombre del área (gerencia) asociada al acceso especial.
        /// </summary>
        public string NombreGerencia { get; set; } = string.Empty;

        /// <summary>
        /// Identificador único del área (departamento) asociado al acceso especial (opcional).
        /// </summary>
        public long? IdDepartamento { get; set; }

        /// <summary>
        /// Nombre del área (departamento) asociado al acceso especial (opcional).
        /// </summary>
        public string? NombreDepartamento { get; set; }

        /// <summary>
        /// Estado actual del acceso especial.
        /// </summary>
        public string Estado { get; set; } = string.Empty;

        /// <summary>
        /// Fecha de creación del acceso especial.
        /// </summary>
        public DateTime FechaCreacion { get; set; }
    }

}
