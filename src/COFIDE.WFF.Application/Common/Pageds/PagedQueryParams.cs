namespace COFIDE.WFF.Application.Common.Pageds;

/// <summary>
/// Representa los parámetros de entrada para realizar una paginación estándar.
/// Incluye control de página, tamaño, ordenamiento y búsqueda.
/// </summary>
public class PagedQueryParams
{
    private const int MaxPageSize = PagedDefaults.MaxPageSize;

    private int _pageSize = PagedDefaults.DefaultPageSize;

    /// <summary>
    /// Número de página solicitada. Por defecto, <c>1</c>.
    /// </summary>
    public int PageNumber { get; set; } = PagedDefaults.DefaultPageNumber;

    /// <summary>
    /// Cantidad de registros por página. Máximo permitido: <c>100</c>.
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }

    /// <summary>
    /// Lista de campos para ordenamiento múltiple (cada uno con su dirección).
    /// Reemplaza a SortBy si se provee.
    /// </summary>
    public List<(string PropertyName, bool Descending)>? Sorts { get; set; }

    /// <summary>
    /// Término de búsqueda libre para filtrar resultados (opcional).
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Normaliza los valores por si se recibe un número de página o tamaño inválido.
    /// </summary>
    public void Normalize()
    {
        if (PageNumber <= 0) PageNumber = PagedDefaults.DefaultPageNumber;
        if (PageSize <= 0) PageSize = PagedDefaults.DefaultPageSize;

        // Si después sigue vacía, dejarla null para evitar procesamiento innecesario
        if (Sorts is { Count: 0 })
            Sorts = null;
    }
}
