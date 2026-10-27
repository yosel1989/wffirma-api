namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerGestionSolicitudes
{

    /// <summary>
    /// Representa una solicitud en el listado de gestión de solicitudes.
    /// </summary>
    public class ObtenerGestionSolicitudesDto
    {
        /// <summary>Identificador único de la solicitud.</summary>
        public long IdSolicitud { get; set; }

        /// <summary>Código visible de la solicitud.</summary>
        public string? CodigoSolicitud { get; set; }

        /// <summary>Nombre de la persona que registró la solicitud.</summary>
        public string? Solicitante { get; set; }

        /// <summary>Nombre de la última persona que firmo la solicitud.</summary>
        public string? AprobadorActual { get; set; }

        /// <summary>Estado actual de la solicitud.</summary>
        public string? Estado { get; set; }

        /// <summary>Fecha y hora de creación de la solicitud.</summary>
        public DateTime FechaCreacion { get; set; }

        /// <summary>Fecha y hora de creación de la solicitud.</summary>
        public DateTime? FechaModificacion { get; set; }

    }

    /// <summary>
    /// Respuesta paginada de la consulta de gestión de solicitudes: incluye los
    /// contadores por estado, el listado de la página actual y los datos de paginación.
    /// </summary>
    public class ObtenerGestionSolicitudesResponseDto
    {
        /// <summary>Cantidad total de solicitudes.</summary>
        public long Total { get; set; }

        /// <summary>Solicitudes correspondientes a la página actual.</summary>
        public IEnumerable<ObtenerGestionSolicitudesDto> ListadoSolicitudes { get; set; } = [];

        /// <summary>Número de la página actual (empieza en 1).</summary>
        public int PageNumber { get; set; }

        /// <summary>Cantidad de registros por página.</summary>
        public int PageSize { get; set; }

        /// <summary>Cantidad total de registros que cumplen el filtro, sin paginar.</summary>
        public int TotalCount { get; set; }

        /// <summary>Total de páginas, calculado a partir de <see cref="TotalCount"/> y <see cref="PageSize"/>.</summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        /// <summary>Indica si existe una página anterior.</summary>
        public bool HasPrevious => PageNumber > 1;

        /// <summary>Indica si existe una página siguiente.</summary>
        public bool HasNext => PageNumber < TotalPages;
    }

    /// <summary>
    /// Contador general de la gestión de solicitudes.
    /// </summary>
    public class GestionSolicitudesResumenDto
    {
        /// <summary>Cantidad total de solicitudes que cumplen los filtros aplicados.</summary>
        public long Total { get; set; }
    }
}