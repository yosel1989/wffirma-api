namespace COFIDE.WFF.Application.Ports.Out;

public interface IHtmlTemplateRenderer
{
    string Render(string templateCode, object model);
}
