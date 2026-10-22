using COFIDE.WFF.Application.Common.Queries;

namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerGestionSolicitudes
{
    /// <summary>
    /// Consulta paginada para obtener el listado de gestión de solicitudes,
    /// con filtros opcionales y, si se requiere, los contadores por estado.
    /// </summary>
    /// <param name="Fechainicio">Fecha inicial del rango de creación de la solicitud.</param>
    /// <param name="Fechafin">Fecha final del rango de creación de la solicitud.</param>
    /// <param name="Gerencia">Identificadores de las gerencias a filtrar.</param>
    /// <param name="Departamento">Identificadores de los departamentos a filtrar.</param>
    /// <param name="Solicitante">Identificadores de los solicitantes a filtrar.</param>
    /// <param name="Estado">Identificadores de los estados de solicitud a filtrar.</param>
    /// <param name="Firmante">Identificadores de los firmantes a filtrar.</param>
    /// <param name="Codigosolicitud">Código de la solicitud a buscar.</param>
    public sealed record ObtenerGestionSolicitudesQuery(
        string? Fechainicio,
        string? Fechafin,
        List<long>? Gerencia,
        List<long>? Departamento,
        List<long>? Solicitante,
        List<long>? Firmante,
        List<long>? Estado,
        string? Codigosolicitud
    ) : GetPagedQuery<ObtenerGestionSolicitudesResponseDto>;
}