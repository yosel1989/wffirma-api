namespace COFIDE.WFF.Application.Common.Ports.Out;

public interface IHtmlTemplateRenderer
{
    string Render(string templateCode, object model);
}
