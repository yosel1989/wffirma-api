using COFIDE.WFF.Domain.Common.Pageds;
using COFIDE.WFF.Domain.Common.Enums;
namespace COFIDE.WFF.Application.Features.Plantillas.Queries.ObtenerPlantillaPaginado;

public class PlantillaPaginadoRequest
{
    public PagedQueryParams PaginationParams { get; set; } = default!;
    public string? Busqueda { get; set; } = default!;
    public bool Visibilidad { get; set; } = default!;
    public List<long>? Aprobadores { get; set; } = default!;
    public List<string>? Gerencias { get; set; } = default!;
    public RolUsuario RolUsuario { get; set; } = default!;
    public string AreaParaCoordinador { get; set; } = default!;
    public long IdUsuarioPropietario { get; set; } = default!;
}
