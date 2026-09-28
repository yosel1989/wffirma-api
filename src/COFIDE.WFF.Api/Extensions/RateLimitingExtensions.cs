using COFIDE.WFF.Api.Configuration;
using System.Threading.RateLimiting;

namespace COFIDE.WFF.Api.Extensions;

public static class RateLimitingExtensions
{
    private const string GlobalPolicyName = "GlobalPolicy";

    public static IServiceCollection AddRateLimitingConfig(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration
            .GetSection("RateLimiting")
            .Get<RateLimitOptions>()
            ?? throw new InvalidOperationException("RateLimiting no configurado");

        services.AddRateLimiter(rateOptions =>
        {
            rateOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            rateOptions.AddPolicy(GlobalPolicyName, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = options.PermitLimit,
                        Window = TimeSpan.FromSeconds(options.WindowInSeconds),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = options.QueueLimit
                    }));
        });

        return services;
    }

    public static IApplicationBuilder UseRateLimitingConfig(
        this IApplicationBuilder app)
    {
        app.UseRateLimiter();
        return app;
    }

    public static IEndpointRouteBuilder MapRateLimitedControllers(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapControllers().RequireRateLimiting(GlobalPolicyName);
        return endpoints;
    }
}