using COFIDE.WFF.Application.Common.Constants;
using COFIDE.WFF.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
namespace COFIDE.WFF.Infrastructure.Context;

public sealed class CorrelationContext(
        IHttpContextAccessor accessor
    ) : ICorrelationContext
{
    private readonly IHttpContextAccessor _accessor = accessor;

    public string CorrelationId =>
        _accessor.HttpContext?.Items[CorrelationConstants.HeaderName]?.ToString()
        ?? string.Empty;
}