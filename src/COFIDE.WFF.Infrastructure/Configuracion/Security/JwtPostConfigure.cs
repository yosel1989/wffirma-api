using COFIDE.WFF.Infrastructure.Configuracion.Runtime;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;
namespace COFIDE.WFF.Infrastructure.Configuracion.Security;

public sealed class JwtPostConfigure
    : IPostConfigureOptions<JwtBearerOptions>
{
    private readonly IConfiguration _configuration;
    private readonly IRuntimeConfigurationProvider _runtime;

    public JwtPostConfigure(
        IConfiguration configuration,
        IRuntimeConfigurationProvider runtime)
    {
        _configuration = configuration;
        _runtime = runtime;
    }

    public void PostConfigure(
        string? name,
        JwtBearerOptions options)
    {
        var issuer =
            _configuration["JwtSettings:Issuer"];

        var audience =
            _configuration["JwtSettings:Audience"];

        var secret =
            _runtime
            .GetJwtSecretAsync()
            .GetAwaiter()
            .GetResult();

        var bytes =
            Encoding.ASCII.GetBytes(secret);

        if (bytes.Length < 32)
            Array.Resize(ref bytes, 32);

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = issuer,

                ValidAudience = audience,

                IssuerSigningKey =
                    new SymmetricSecurityKey(bytes),

                ClockSkew = TimeSpan.Zero
            };
    }
}