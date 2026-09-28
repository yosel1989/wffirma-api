namespace COFIDE.WFF.Infrastructure.Configuration.External;

public interface ISucConfigurationClient
{
    Task<string> ObtenerConfiguracionAsync(
        string codigoConfiguracion,
        CancellationToken cancellationToken = default);
}