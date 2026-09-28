namespace COFIDE.WFF.Infrastructure.Configuracion.Runtime;

public interface IRuntimeConfigurationProvider
{
    Task<string> GetOracleConnectionAsync(CancellationToken cancellationToken = default);

    Task<string> GetJwtSecretAsync(CancellationToken cancellationToken = default);
}