using static System.Net.Mime.MediaTypeNames;

namespace COFIDE.WFF.Application.Common.Pageds;

/// <summary>
/// Representa un conjunto de resultados paginados dentro del dominio.
/// Incluye la colección de datos y los metadatos esenciales de paginación.
/// </summary>
/// <typeparam name="T">Tipo de elemento contenido en la paginación.</typeparam>
public class PagedResult<T, TExtra>
{
    /// <summary>
    /// Colección inmutable de elementos para la página actual.
    /// </summary>
    public IEnumerable<T> Items { get; }
    /// <summary>
    /// Información adicional para el paginado
    /// </summary>
    public TExtra? ExtraData { get; set; }

    /// <summary>
    /// Número total de elementos encontrados (sin paginar).
    /// </summary>
    public int TotalCount { get; }

    /// <summary>
    /// Página actual (comienza desde 1).
    /// </summary>
    public int PageNumber { get; }

    /// <summary>
    /// Cantidad de elementos por página.
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// Total de páginas disponibles, basado en <see cref="TotalCount"/> y <see cref="PageSize"/>.
    /// </summary>
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Constructor principal para crear un resultado paginado.
    /// </summary>
    public PagedResult(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items ?? Enumerable.Empty<T>();
        TotalCount = totalCount;
        PageNumber = pageNumber < 1 ? PagedDefaults.DefaultPageNumber : pageNumber;
        PageSize = pageSize <= 0 ? PagedDefaults.DefaultPageSize : pageSize;
    }

    /// <summary>
    /// Constructor alternativo cuando no se requiere paginación explícita.
    /// </summary>
    public PagedResult(IEnumerable<T> items, int totalCount)
        : this(items, totalCount, PagedDefaults.DefaultPageNumber, items?.Count() ?? 0)
    {
    }
}


public class PagedResult<T> : PagedResult<T, object?>
{
    public PagedResult(
        IEnumerable<T> items,
        int totalCount,
        int pageNumber,
        int pageSize)
        : base(items, totalCount, pageNumber, pageSize)
    {
    }

    public PagedResult(
        IEnumerable<T> items,
        int totalCount)
        : base(items, totalCount)
    {
    }
}