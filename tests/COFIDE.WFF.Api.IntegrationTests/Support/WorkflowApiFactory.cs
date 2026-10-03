using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Infrastructure.Persistence.Contexts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace COFIDE.WFF.Api.IntegrationTests.Support;

/// <summary>
/// Levanta la Api completa en memoria con:
///   - Base de datos de PRUEBAS (cadena de ConfiguracionPruebas, no la obtenida del SUC).
///   - Autenticación de prueba en lugar de JWT.
///   - Correo y notificaciones en tiempo real reemplazados por dobles que solo registran (nunca envían).
///   - Sin reintentos de publicación de eventos en segundo plano.
/// </summary>
public sealed class WorkflowApiFactory : WebApplicationFactory<Program>
{
    public CorreoDePrueba Correos { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTests");

        builder.ConfigureAppConfiguration((_, config) =>
            config.AddConfiguration(ConfiguracionPruebas.Configuracion));

        builder.ConfigureTestServices(services =>
        {
            // Base de datos de pruebas: se elimina la configuración original (que pide la cadena al SUC).
            services.RemoveAll<DbContextOptions<WorkflowFirmaDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<WorkflowFirmaDbContext>>();
            services.AddDbContext<WorkflowFirmaDbContext>(options =>
                options.UseOracle(ConfiguracionPruebas.CadenaConexion ?? "Data Source=sin-configurar"));

            // Autenticación de prueba.
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = AutenticacionDePrueba.Esquema;
                    options.DefaultChallengeScheme = AutenticacionDePrueba.Esquema;
                    options.DefaultScheme = AutenticacionDePrueba.Esquema;
                })
                .AddScheme<AuthenticationSchemeOptions, AutenticacionDePrueba>(AutenticacionDePrueba.Esquema, _ => { });

            // Efectos externos: nunca se envían correos ni notificaciones reales desde las pruebas.
            services.RemoveAll<ICorreoSenderPort>();
            services.AddSingleton<ICorreoSenderPort>(Correos);
            services.RemoveAll<INotificacionTiempoRealPort>();
            services.AddSingleton<INotificacionTiempoRealPort, NotificacionTiempoRealDePrueba>();
        });
    }

    /// <summary>Cliente HTTP autenticado como el usuario de prueba.</summary>
    public HttpClient ClienteAutenticado()
    {
        var cliente = CreateClient();
        cliente.DefaultRequestHeaders.Add(AutenticacionDePrueba.Cabecera, "1");
        return cliente;
    }

    /// <summary>Scope de servicios para pruebas de persistencia (sin pasar por HTTP).</summary>
    public AsyncServiceScope CrearScope() => Services.CreateAsyncScope();
}
