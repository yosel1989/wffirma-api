using COFIDE.WFF.Application.Common.Pageds;
using COFIDE.WFF.Application.Common.Queries;
namespace COFIDE.WFF.Application.Features.Plantillas.Queries.ObtenerPlantillaPaginado;

public sealed record ObtenerPlantillaPaginadoQuery
(
    string? busqueda,
    bool Visibilidad,
    List<long>? aprobadores,
    List<string>? gerencias
) : GetPagedQuery<PagedResultDto<PlantillaPaginadoDto>>;
