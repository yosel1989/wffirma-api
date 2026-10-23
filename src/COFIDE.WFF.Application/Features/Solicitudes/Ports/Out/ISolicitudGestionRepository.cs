

using COFIDE.WFF.Application.Common.Pageds;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerGestionSolicitudes;

namespace COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;

public interface ISolicitudGestionRepository
{
    Task<PagedResult<ObtenerGestionSolicitudesDto, GestionSolicitudesResumenDto>> ObtenerPaginadoGestionSeguimiento(
        PagedQueryParams paginationParams,
        long? codigoUsuario,
        long? areaUsuario,
        string? roLUsuario,
        long? departamentoUsuario,
        string? fechainicio,
        string? fechafin,
        List<long>? gerencia,
        List<long>? departamento,
        List<long>? solicitante,
        List<long>? firmante,
        List<long>? estado,
        string? codigosolicitud,
        CancellationToken cancellationToken
    );
}
