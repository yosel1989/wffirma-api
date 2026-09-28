using COFIDE.WFF.Infrastructure.Adapters.ExternalServices.Suc;
using COFIDE.WFF.Infrastructure.Bootstrap;
namespace COFIDE.WFF.Infrastructure.Providers;


public sealed class HubApiKeyProvider(
    SucApiClient client
) : IHubApiKeyProvider
{

    private readonly SucApiClient _client = client;


    public async Task<SecurityConfiguration> ObtenerApiKeyAsync(
        CancellationToken cancellationToken = default)
    {


        var parametros =
            await _client.ObtenerParametroDetalleAsync(
                SecurityParameterCodes.HubApiKey,
                cancellationToken);


        string ObtenerValor(string codigo)
        {
            return parametros
                .First(x =>
                    x.CodigoConfiguracion.ToUpper() == codigo.ToUpper())
                .ValConf1!;
        }


        return new SecurityConfiguration
        {
            HubApiKey =
                ObtenerValor(
                    SecurityParameterCodes.HubApiKey)
        };
    }
}
