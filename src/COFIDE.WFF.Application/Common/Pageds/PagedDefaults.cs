namespace COFIDE.WFF.Application.Common.Pageds;

/// <summary>
/// Contiene valores por defecto usados para la configuración de paginación.
/// Aplica directamente a <see cref="PagedQueryParams"/>.
/// </summary>
public static class PagedDefaults
{
    /// <summary>
    /// Página inicial por defecto (1-based).
    /// </summary>
    public const int DefaultPageNumber = 1;

    /// <summary>
    /// Tamaño de página por defecto (cantidad de elementos por página).
    /// </summary>
    public const int DefaultPageSize = 10;

    /// <summary>
    /// Tamaño máximo permitido para una página.
    /// Previene sobrecarga o abuso en consultas masivas.
    /// </summary>
    public const int MaxPageSize = 100;

    /// <summary>
    /// Campo de ordenamiento por defecto (opcional, según entidad).
    /// </summary>
    public const string DefaultSortBy = "FechaCreacion";

    /// <summary>
    /// Dirección de ordenamiento por defecto: ascendente (false).
    /// </summary>
    public const bool DefaultSortDescending = false;
}
