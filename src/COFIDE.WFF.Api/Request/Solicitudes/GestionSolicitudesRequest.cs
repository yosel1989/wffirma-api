using COFIDE.WFF.Api.Request.Comun;

namespace COFIDE.WFF.Api.Request.Solicitudes
{
    /// <summary>
    /// Parámetros de entrada para consultar el listado paginado de gestión de solicitudes.
    /// Todos los filtros son opcionales; los que no se envíen no se aplican.
    /// </summary>
    public sealed class GestionSolicitudesRequest : PaginadoRequest
    {
        /// <summary>Fecha inicial del rango de creación de la solicitud.</summary>
        public string? Fechainicio { get; init; }

        /// <summary>Fecha final del rango de creación de la solicitud.</summary>
        public string? Fechafin { get; init; }

        /// <summary>Identificadores de las gerencias a filtrar.</summary>
        public List<long>? Gerencia { get; init; }

        /// <summary>Identificadores de los departamentos a filtrar.</summary>
        public List<long>? Departamento { get; init; }

        /// <summary>Identificadores de los solicitantes a filtrar.</summary>
        public List<long>? Solicitante { get; init; }

        /// <summary>Identificadores de los firmantes a filtrar.</summary>
        public List<long>? Firmante { get; init; }

        /// <summary>Identificadores de los estados de solicitud a filtrar.</summary>
        public List<long>? Estado { get; init; }

        /// <summary>Código de la solicitud a buscar.</summary>
        public string? Codigosolicitud { get; init; }
    }
}