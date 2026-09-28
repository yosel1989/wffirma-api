using COFIDE.WFF.Application.Features.Solicitudes.DTOs;
namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerSolicitudPaginado;

public class SolicitudResumenPaginadoDto
{
    public long MisSolicitudes { get; set; }
    public long Total { get; set; }
    public long MiDepartamento { get; set; }
    public long Urgente { get; set; }
}


public class SolicitudPaginadoResponseDto
{
    public long MisSolicitudes { get; set; }
    public long Total { get; set; }
    public long MiDepartamento { get; set; }
    public long Urgente { get; set; }
    public IEnumerable<SolicitudPaginadoDto> ListadoSolicitudItem { get; set; } = [];
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;
}