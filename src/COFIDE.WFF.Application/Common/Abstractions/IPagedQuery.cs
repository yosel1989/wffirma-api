using COFIDE.WFF.Application.Common.Pageds;
namespace COFIDE.WFF.Application.Common.Abstractions;

/// <summary>
/// Contrato para queries que admiten paginación, ordenamiento y filtros estándar.
/// Permite trabajar de forma polimórfica con queries que exponen parámetros de tipo <see cref="PagedQueryParamsDto"/>.
/// </summary>
public interface IPagedQuery
{
    int PageNumber { get; }

    /// <summary>
    /// Número de página (mínimo 1). Por defecto: 1.
    /// </summary>
    int PageSize { get; }

    /// <summary>
    /// Lista de campos para ordenación múltiple.
    /// </summary>
    List<SortDto>? Sorts { get; }

    /// <summary>
    /// Término general de búsqueda (opcional).
    /// </summary>
    string? SearchTerm { get; }
}
