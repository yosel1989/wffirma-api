using Microsoft.AspNetCore.Cors.Infrastructure;
namespace COFIDE.WFF.Api.Configuration;

/// <summary>
/// Clase de extensión para configurar la política CORS (Cross-Origin Resource Sharing) de la API.
/// Permite definir orígenes permitidos, métodos HTTP, encabezados y uso de credenciales.
/// </summary>
public static class CorsConfig
{
    private const string CorsPolicyName = "DefaultCorsPolicy";

    /// <summary>
    /// Expone el nombre de la política CORS predeterminada para usar en middleware o controladores.
    /// </summary>
    public static string DefaultPolicyName => CorsPolicyName;

    /// <summary>
    /// Agrega la configuración de CORS al contenedor de servicios.
    /// La política se llama "DefaultCorsPolicy" y se basa en los orígenes definidos en el archivo de configuración (clave: "AllowedOrigins").
    /// </summary>
    /// <param name="services">Contenedor de servicios de la aplicación.</param>
    /// <param name="config">Instancia de configuración (IConfiguration), usada para leer los orígenes permitidos.</param>
    /// <returns>El mismo IServiceCollection para permitir el encadenamiento de métodos.</returns>    
    public static IServiceCollection AddCorsConfig(this IServiceCollection services, IConfiguration config)
    {
        var optionsCors = new CorsOptions();
        config.GetSection("CorsOptions").Bind(optionsCors);

        services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicyName, policy =>
            {
                policy
                    .WithOrigins(optionsCors.Origins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return services;
    }
}
