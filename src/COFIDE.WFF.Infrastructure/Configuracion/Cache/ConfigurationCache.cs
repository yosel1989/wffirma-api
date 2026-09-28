using COFIDE.WFF.Infrastructure.Configuration.External;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace COFIDE.WFF.Infrastructure.Configuracion.Cache;

public sealed class ConfigurationCache : IConfigurationCache
{
    private readonly IMemoryCache _cache;
    private readonly ISucConfigurationClient _client;
    private readonly ILogger<ConfigurationCache> _logger;

    public ConfigurationCache(
        IMemoryCache cache,
        ISucConfigurationClient client,
        ILogger<ConfigurationCache> logger)
    {
        _cache = cache;
        _client = client;
        _logger = logger;
    }

    public async Task<string> GetAsync(
        string codigo,
        CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(codigo, out string? valor))
            return valor!;

        _logger.LogInformation(
            "Obteniendo configuración {Codigo} desde SUC.",
            codigo);

        valor = await ExecuteRetryAsync(() =>
            _client.ObtenerConfiguracionAsync(
                codigo,
                cancellationToken));

        _cache.Set(
            codigo,
            valor,
            TimeSpan.FromMinutes(30));

        return valor;
    }

    public async Task RefreshAsync(
        string codigo,
        CancellationToken cancellationToken = default)
    {
        _cache.Remove(codigo);

        await GetAsync(
            codigo,
            cancellationToken);
    }

    public Task ClearAsync()
    {
        if (_cache is MemoryCache memoryCache)
        {
            memoryCache.Clear();
        }

        return Task.CompletedTask;
    }

    private async Task<string> ExecuteRetryAsync(
        Func<Task<string>> action)
    {
        Exception? last = null;

        for (int i = 1; i <= 5; i++)
        {
            try
            {
                return await action();
            }
            catch (Exception ex)
            {
                last = ex;

                _logger.LogWarning(
                    ex,
                    "Intento {Intento}/5 obteniendo configuración.",
                    i);

                await Task.Delay(TimeSpan.FromSeconds(i * 2));
            }
        }

        throw last!;
    }
}