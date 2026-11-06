namespace COFIDE.WFF.Infrastructure.Adapters.ExternalServices.Correo;
public class CorreoApiOptions
{
    public string BaseUrl { get; set; } = default!;
    public string CodigoSistema { get; set; } = default!;
    public string SenderAlias { get; set; } = default!;
}