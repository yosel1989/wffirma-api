namespace COFIDE.WFF.Infrastructure.Configuracion.Cache;

public interface IConfigurationCache
{
    Task<string> GetAsync(
        string codigo,
        CancellationToken cancellationToken = default);

    Task RefreshAsync(
        string codigo,
        CancellationToken cancellationToken = default);

    Task ClearAsync();
}