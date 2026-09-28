using COFIDE.WFF.Infrastructure.Bootstrap;
using COFIDE.WFF.Infrastructure.Configuracion.Cache;
using Microsoft.Extensions.Options;
namespace COFIDE.WFF.Infrastructure.Configuracion.Runtime;

public sealed class RuntimeConfigurationProvider(
        IConfigurationCache cache,
        IOptions<SucWcfOptions> options
    ) : IRuntimeConfigurationProvider
{
    private readonly IConfigurationCache _cache = cache;
    private readonly SucWcfOptions _options = options.Value;

    public Task<string> GetOracleConnectionAsync(
        CancellationToken cancellationToken = default)
    {
        return _cache.GetAsync(
           _options.CodigoCadena,
            cancellationToken);
    }

    public Task<string> GetJwtSecretAsync(
        CancellationToken cancellationToken = default)
    {
        return _cache.GetAsync(
             _options.JwtSecretKey,
            cancellationToken);
    }
}