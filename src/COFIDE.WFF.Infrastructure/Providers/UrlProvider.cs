using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using Microsoft.Extensions.Configuration;
namespace COFIDE.WFF.Infrastructure.Providers;

public class UrlProvider : IUrlProvider
{
    private readonly IConfiguration _config;

    public UrlProvider(IConfiguration config)
    {
        _config = config;
    }

    public string GetWorkflowBaseFrontUrl()
    {
        return _config["FrontEnd:BaseUrl"]!;
    }
    public string GetWorkflowBaseBackUrl()
    {
        return _config["BackEnd:BaseUrl"]!;
    }
}
