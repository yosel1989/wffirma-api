using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace COFIDE.WFF.Api.Configuration;

public class ConfigureSwaggerOptions(
        IApiVersionDescriptionProvider provider
    ) : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider = provider;

    public void Configure(SwaggerGenOptions options)
    {
        var buildDate = File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location);

        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, new OpenApiInfo
            {
                Title = "COFIDE.WFF.API",
                Version = description.ApiVersion.ToString(),
                Description = "API para autenticación y autorización de COFIDE\n\n" +
                 $"Release: {buildDate:yyyy-MM-dd HH:mm:ss}"
            });
        }
    }
}