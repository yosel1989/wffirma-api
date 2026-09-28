using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
namespace COFIDE.WFF.Infrastructure.Security;

public class SucAuthDelegatingHandler(
        IHttpContextAccessor httpContextAccessor
    ) : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor= httpContextAccessor;

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = _httpContextAccessor.HttpContext?
            .Request
            .Headers
            .Authorization
            .ToString();

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization =
                AuthenticationHeaderValue.Parse(token);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
