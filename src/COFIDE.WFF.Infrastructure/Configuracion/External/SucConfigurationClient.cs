using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Infrastructure.Adapters.ExternalServices.ConfiguracionSuc.Contracts;
using COFIDE.WFF.Infrastructure.Adapters.ExternalServices.Suc;
using COFIDE.WFF.Infrastructure.Bootstrap;
using COFIDE.WFF.Infrastructure.Configuration.External;
using COFIDE.WFF.Infrastructure.Exceptions;
using Microsoft.Extensions.Options;
using System.ServiceModel;
namespace COFIDE.WFF.Infrastructure.Configuracion.External;

public sealed class SucConfigurationClient(
        IOptions<SucWcfOptions> options
    ) : ISucConfigurationClient
{
    private readonly SucWcfOptions _options = options.Value;


    public async Task<string> ObtenerConfiguracionAsync(string codigoConfiguracion,
   CancellationToken cancellationToken = default)
    {
        var binding = new BasicHttpBinding(BasicHttpSecurityMode.None)
        {
            MaxReceivedMessageSize = 1024 * 1024
        };

        var endpoint = new EndpointAddress(_options.BaseUrl);

        var factory =
            new ChannelFactory<IConfiguracionSucService>(
                binding,
                endpoint);

        var client = factory.CreateChannel();

        try
        {
            var dto = await client.obtenerCadenaConexionAsync(
              _options.CodigoSistema,
              codigoConfiguracion);

            ((ICommunicationObject)client).Close();
            factory.Close();

            if (dto == null)
                throw new InfrastructureException(SucErrors.SucWcfErrorsNull);

            if (string.IsNullOrWhiteSpace(dto.strValor))
                throw new InfrastructureException(new ApplicationError("SUC_WCF_EMPTY_RESPONSE", $"La configuración '{codigoConfiguracion}' vino vacía."));

            return dto.strValor;
        }
        catch
        {
            ((ICommunicationObject)client).Abort();
            factory.Abort();
            throw;
        }
    }
}
