namespace COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;

public interface IUrlProvider
{
    string GetWorkflowBaseFrontUrl();
    string GetWorkflowBaseBackUrl();
}
