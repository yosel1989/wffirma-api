using COFIDE.WFF.Application.Common.Ports.Out;

namespace COFIDE.WFF.Infrastructure.Adapters.Services.Template;
public sealed class HtmlTemplateRenderer: IHtmlTemplateRenderer
{
    public string Render(string templateCode, object model)
    {
        var result = templateCode;

        var properties = model.GetType().GetProperties();

        foreach (var prop in properties)
        {
            var placeholder = $"{{{{{prop.Name}}}}}";
            var value = prop.GetValue(model)?.ToString() ?? string.Empty;

            result = result.Replace(placeholder, value);
        }

        return result;
    }
}
