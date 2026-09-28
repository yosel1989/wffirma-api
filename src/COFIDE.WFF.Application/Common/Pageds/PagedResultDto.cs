namespace COFIDE.WFF.Application.Common.Pageds;


/// <summary>
/// Representa un conjunto de resultados paginados con metadatos útiles para la navegación.
/// </summary>
/// <typeparam name="T">Tipo de los elementos contenidos.</typeparam>
public class PagedResultDto<T>
{
    /// <summary>
    /// Lista de elementos de la página actual.
    /// </summary>
    public IEnumerable<T> Items { get; set; }

    public object? ExtraData { get; set; }

    /// <summary>
    /// Número de la página actual (1-indexado).
    /// </summary>
    public int PageNumber { get; init; }

    /// <summary>
    /// Tamaño de la página (cantidad de elementos por página).
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Número total de elementos en la colección completa.
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Número total de páginas calculado en base al total de elementos y el tamaño de página.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary>
    /// Indica si existe una página anterior a la actual.
    /// </summary>
    public bool HasPrevious => PageNumber > 1;

    /// <summary>
    /// Indica si existe una página siguiente a la actual.
    /// </summary>
    public bool HasNext => PageNumber < TotalPages;

    /// <summary>
    /// Constructor principal para resultados paginados.
    /// </summary>
    public PagedResultDto(IEnumerable<T> items, int pageNumber, int pageSize, int totalCount, object? extraData = null)
    {
        Items = items;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
        ExtraData = extraData;
    }

    /// <summary>
    /// Crea una instancia de <see cref="PagedResultDto{T}"/> con los datos provistos.
    /// </summary>
    public static PagedResultDto<T> Create(IEnumerable<T> items, int pageNumber, int pageSize, int totalCount)
    {
        return new PagedResultDto<T>(items, pageNumber, pageSize, totalCount);
    }

    /// <summary>
    /// Crea una instancia vacía de <see cref="PagedResultDto{T}"/> con la paginación definida.
    /// </summary>
    public static PagedResultDto<T> Empty(int pageNumber, int pageSize)
    => new([], pageNumber, pageSize, 0);
}
