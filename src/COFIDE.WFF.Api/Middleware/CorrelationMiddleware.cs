using COFIDE.WFF.Application.Common.Constants;
using Serilog.Context;

namespace COFIDE.WFF.Api.Middleware;
public sealed class CorrelationMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationMiddleware(RequestDelegate next)
        => _next = next;

    public async Task Invoke(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationConstants.HeaderName].FirstOrDefault()
                            ?? context.TraceIdentifier;

        context.Items[CorrelationConstants.HeaderName] = correlationId;
        context.Response.Headers[CorrelationConstants.HeaderName] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}