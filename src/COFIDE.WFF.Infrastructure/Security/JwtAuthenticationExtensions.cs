using COFIDE.WFF.Infrastructure.Configuracion.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
namespace COFIDE.WFF.Infrastructure.Security;
public static class JwtAuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services)
    {

        services.AddAuthentication(options =>
             {
                 options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                 options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                 options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
             })
             .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
             {
                 options.RequireHttpsMetadata = false;
                 options.SaveToken = true;

                 options.Events = new JwtBearerEvents
                 {
                     OnAuthenticationFailed = context =>
                     {
                         return Task.CompletedTask;
                     },
                     OnTokenValidated = context =>
                     {
                         return Task.CompletedTask;
                     },
                     OnMessageReceived = context =>
                     {
                         var authHeader = context.Request.Headers.Authorization.FirstOrDefault();

                         if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                         {
                             context.Token = authHeader["Bearer ".Length..].Trim();
                         }
                         return Task.CompletedTask;
                     },
                 };
             });

        services.AddSingleton<
          IPostConfigureOptions<JwtBearerOptions>,
          JwtPostConfigure>();

        services.AddAuthorization();

        return services;
    }
}
