using COFIDE.WFF.Domain.Common.Pageds;
namespace COFIDE.WFF.Application.Common.Pageds;

/// <summary>
/// DTO para representar los parámetros de una consulta paginada desde UI o API.
/// </summary>
public class PagedQueryParamsDto
{
    public int PageNumber { get; set; } = PagedDefaults.DefaultPageNumber;

    /// <summary>
    /// Número de página (mínimo 1). Por defecto: 1.
    /// </summary>
    public int PageSize { get; set; } = PagedDefaults.DefaultPageSize;

    /// <summary>
    /// Lista de campos para ordenación múltiple.
    /// </summary>
    public List<SortDto>? Sorts { get; set; }

    /// <summary>
    /// Término general de búsqueda (opcional).
    /// </summary>
    public string? SearchTerm { get; set; } = String.Empty;
}