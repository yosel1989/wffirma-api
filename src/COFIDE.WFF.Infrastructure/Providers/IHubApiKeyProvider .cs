using COFIDE.WFF.Infrastructure.Bootstrap;

namespace COFIDE.WFF.Infrastructure.Providers;

public interface IHubApiKeyProvider
{
    Task<SecurityConfiguration> ObtenerApiKeyAsync(
        CancellationToken cancellationToken = default);
}