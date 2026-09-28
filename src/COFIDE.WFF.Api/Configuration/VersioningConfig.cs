using Asp.Versioning;

namespace COFIDE.WFF.Api.Configuration;

public static class VersioningConfig
{
    public static IServiceCollection AddVersioningConfig(this IServiceCollection services)
    {
        var apiVersioningBuilder = services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;

            // /api/v1/...
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        });

        apiVersioningBuilder.AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV"; // v1, v2, v1.1
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }
}
