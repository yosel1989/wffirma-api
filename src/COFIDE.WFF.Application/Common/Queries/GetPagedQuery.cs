using COFIDE.WFF.Application.Common.Abstractions;
using COFIDE.WFF.Application.Common.Pageds;
namespace COFIDE.WFF.Application.Common.Queries;


/// <summary>
/// Clase base abstracta para consultas paginadas en operaciones de solo lectura.
/// Proporciona la estructura común para manejar parámetros de paginación, ordenamiento y filtrado.
/// </summary>
/// <typeparam name="TResponse">Tipo de la respuesta esperada, usualmente un DTO que contiene resultados paginados.</typeparam>
public abstract record GetPagedQuery<TResponse> : BaseQuery<TResponse>, IPagedQuery
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
