using COFIDE.WFF.Application.Common.Pageds;
using COFIDE.WFF.Application.Features.Plantillas.Queries.GestionPlantillas;

namespace COFIDE.WFF.Application.Features.Plantillas.Ports.Out;

public interface IPlantillaGestionRepository
{
    Task<PagedResult<GestionPlantillasDto>> ObtenerPaginadoGestion(
        PagedQueryParams paginationParams,
        long? codigoUsuario,
        long? areaUsuario,
        string? rolUsuario,
        string? fechainicio,
        string? fechafin,
        List<long>? gerencia,
        List<long>? departamento,
        List<long>? creador,
        List<long>? aprobador,
        string? nombrePlantilla,
        CancellationToken cancellationToken);
}
