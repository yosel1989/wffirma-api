namespace COFIDE.WFF.Infrastructure.Bootstrap;

public sealed class SucWcfOptions
{
    public const string SectionName = "ExternalServices:SucWcf";
    public string BaseUrl { get; init; } = string.Empty;
    public string CodigoSistema { get; init; } = string.Empty;
    public string CodigoCadena { get; init; } = string.Empty;
    public string JwtSecretKey { get; init; } = string.Empty;
}