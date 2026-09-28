namespace COFIDE.WFF.Infrastructure.Adapters.ExternalServices.Firmas;
public class FirmaApiOptions
{
    public string BaseUrl { get; set; } = default!;
    public string CodigoSistema { get; set; } = default!;
    public string EndpointCallBack { get; set; } = default!;
}
