using COFIDE.WFF.Application.Common.Pageds;

namespace COFIDE.WFF.Api.Request.Comun
{
    /// <summary>Parámetros de paginación y ordenamiento comunes a los listados.</summary>
    public abstract class PaginadoRequest
    {
        public int PageNumber { get; init; } = PagedDefaults.DefaultPageNumber;
        public int PageSize { get; init; } = PagedDefaults.DefaultPageSize;
        public List<SortDto>? Sorts { get; init; }
    }
}
