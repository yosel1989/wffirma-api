using System.Text.Json.Serialization;
using COFIDE.WFF.Domain.Aggregates.AccesoEspecialAggregate.Vo;

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
        /// Código único del área asociada al acceso especial.
        /// </summary>
        public string CodigoArea { get; set; } = string.Empty;

        /// <summary>
        /// Identificador único del área (gerencia y/o departamento) asociada al acceso especial.
        /// </summary>
        public long IdArea { get; set; }

        /// <summary>
        /// Nombre del área (gerencia y/o departamento) asociada al acceso especial.
        /// </summary>
        public string NombreArea { get; set; } = string.Empty;

        /// <summary>
        /// Código del estado.
        /// </summary>
        [JsonIgnore]
        public EstadoCodigo CodigoEstado { get; set; }

        /// <summary>true = Activo, false = Inactivo (se calcula, solo get).</summary>
        public bool Estado => CodigoEstado.Equals(EstadoCodigo.Activo);

        /// <summary>
        /// Fecha de creación del acceso especial.
        /// </summary>
        public DateTime FechaCreacion { get; set; }
    }

}
