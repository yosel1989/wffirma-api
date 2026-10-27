using COFIDE.WFF.Application.Features.AccesosEspeciales.DTOs;

namespace COFIDE.WFF.Application.Features.AccesosEspeciales.Queries.ObtenerAccesoEspecialPaginado
{
    public class AccesoEspecialResumenPaginadoDto
    {
        public long Total { get; set; }
    }


    public class AccesoEspecialPaginadoResponseDto
    {
        public long Total { get; set; }
        public IEnumerable<AccesoEspecialPaginadoDto> Items { get; set; } = [];
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPrevious => PageNumber > 1;
        public bool HasNext => PageNumber < TotalPages;
    }
}
