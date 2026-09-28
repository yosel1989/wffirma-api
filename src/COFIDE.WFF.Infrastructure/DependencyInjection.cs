using COFIDE.WFF.Application.Common.Interfaces;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Firmas.Ports.Out;
using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Features.Usuarios.Ports.Out;
using COFIDE.WFF.Application.Ports.Out;
using COFIDE.WFF.Infrastructure.Adapters.ExternalServices.Correo;
using COFIDE.WFF.Infrastructure.Adapters.ExternalServices.Firmas;
using COFIDE.WFF.Infrastructure.Adapters.ExternalServices.Realtime;
using COFIDE.WFF.Infrastructure.Adapters.ExternalServices.Suc;
using COFIDE.WFF.Infrastructure.Adapters.Repositories;
using COFIDE.WFF.Infrastructure.Adapters.Repositories.Base;
using COFIDE.WFF.Infrastructure.Adapters.Services.FileStorage;
using COFIDE.WFF.Infrastructure.Adapters.Services.Pdf;
using COFIDE.WFF.Infrastructure.Adapters.Services.Reports;
using COFIDE.WFF.Infrastructure.Adapters.Services.Security;
using COFIDE.WFF.Infrastructure.Adapters.Services.Template;
using COFIDE.WFF.Infrastructure.Bootstrap;
using COFIDE.WFF.Infrastructure.Configuracion.Cache;
using COFIDE.WFF.Infrastructure.Configuracion.External;
using COFIDE.WFF.Infrastructure.Configuracion.Runtime;
using COFIDE.WFF.Infrastructure.Configuration.External;
using COFIDE.WFF.Infrastructure.Context;
using COFIDE.WFF.Infrastructure.Persistence.Contexts;
using COFIDE.WFF.Infrastructure.Providers;
using COFIDE.WFF.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace COFIDE.WFF.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        PdfSharpBootstrap.Configure();

        services.AddHttpContextAccessor();
        services.AddScoped<ICorrelationContext, CorrelationContext>();
        services.AddMemoryCache();

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IUsuarioIdentityResolver, UsuarioIdentityResolver>();
        services.AddScoped<IClaimsTransformation, UsuarioClaimsTransformation>();
        services.AddScoped<IUrlProvider, UrlProvider>();

        services.AddScoped<IDocumentoFirmadoValidator, DocumentoFirmadoValidator>();

        services.AddOptions<SucWcfOptions>()
         .Bind(configuration.GetSection("ExternalServices:SucWcf"))
         .Validate(o => !string.IsNullOrWhiteSpace(o.BaseUrl), "BaseUrl SUC WCF es requerido")
         .ValidateOnStart();


        services.AddSingleton<ISucConfigurationClient, SucConfigurationClient>();
        services.AddSingleton<IConfigurationCache, ConfigurationCache>();
        services.AddSingleton<IRuntimeConfigurationProvider, RuntimeConfigurationProvider>();

        #region Base de Datos Oracle

        services.AddDbContext<WorkflowFirmaDbContext>(
       (sp, options) =>
       {
           var runtime =
            sp.GetRequiredService<IRuntimeConfigurationProvider>();

           var connection =
            runtime.GetOracleConnectionAsync()
                   .GetAwaiter()
                   .GetResult();

           options.UseOracle(
            connection,
            oracle =>
           {
               oracle.CommandTimeout(60);
           });
       });

        #endregion

        // JSON Web Token
        services.AddJwtAuthentication();

        #region Servicio Externo SUC
        // DelegatingHandler para Suc
        services.AddTransient<SucAuthDelegatingHandler>();

        services.AddOptions<SucApiOptions>()
            .Bind(configuration.GetSection("ExternalServices:SucApi"))
            .Validate(o => !string.IsNullOrWhiteSpace(o.BaseUrl), "BaseUrl SUC es requerido")
            .ValidateOnStart();

        // Aca estamos agregando el token de manera automática
        services.AddHttpClient<SucApiClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<SucApiOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);

        }).AddHttpMessageHandler<SucAuthDelegatingHandler>();

        services.AddScoped<IAutenticacionExternaPort, SucAutenticacionAdapter>();
        services.AddScoped<IHubApiKeyProvider, HubApiKeyProvider>();
        #endregion

        #region Servicio Externo Correo
        // DelegatingHandler para Correo
        services.AddTransient<CorreoAuthDelegatingHandler>();

        services.AddOptions<CorreoApiOptions>()
          .Bind(configuration.GetSection("ExternalServices:CorreoApi"))
          .Validate(o => !string.IsNullOrWhiteSpace(o.BaseUrl), "BaseUrl Api Correo es requerido")
          .ValidateOnStart();

        // Aca estamos agregando el token de manera automática
        services.AddHttpClient<ICorreoSenderPort, CorreoApiClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<CorreoApiOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);

        }).AddHttpMessageHandler<CorreoAuthDelegatingHandler>();

        #endregion

        #region Servicio Externo Firmas
        // DelegatingHandler para Firmas
        services.AddTransient<FirmaAuthDelegatingHandler>();

        services.AddOptions<FirmaApiOptions>()
         .Bind(configuration.GetSection("ExternalServices:FirmaApi"))
         .Validate(o => !string.IsNullOrWhiteSpace(o.BaseUrl), "BaseUrl Api Firma es requerido")
         .ValidateOnStart();

        // Aca estamos agregando el token de manera automática
        services.AddHttpClient<IFirmaPort, FirmaApiAdapter>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<FirmaApiOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);

        }).AddHttpMessageHandler<FirmaAuthDelegatingHandler>();

        #endregion

        #region Servicio Externo de TiempoReal signalR

        services.AddOptions<HubSocketApiOptions>()
        .Bind(configuration.GetSection("ExternalServices:HubSocketApi"))
        .Validate(o => !string.IsNullOrWhiteSpace(o.BaseUrl), "BaseUrl Api HubSocket es requerido")
        .ValidateOnStart();

        services.AddHttpClient<INotificacionTiempoRealPort, HubSocketRealtimeAdapter>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<HubSocketApiOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        });

        #endregion

        #region Servicio FileStorage
        services.AddOptions<FileStorageOptions>()
                .Bind(configuration.GetSection("FileServer"))
                .Validate(o => !string.IsNullOrWhiteSpace(o.BaseUrl), "BaseUrl FileServer es requerido")
                .ValidateOnStart();
        services.AddScoped<IFileStorageService, FileStorageService>();
        #endregion

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Servicio para modificar el PDF
        services.AddScoped<IPdfCodigoInserter, PdfCodeInserter>();
        services.AddScoped<ISolicitudExcelReportService, SolicitudExcelReportService>();
        services.AddScoped<IPdfContentHashService, PdfContentHashService>();
        services.AddScoped<IOrientacionPaginaPdfService, OrientacionPaginaPdfService>();

        // Servicio para renderizar HTml
        services.AddScoped<IHtmlTemplateRenderer, HtmlTemplateRenderer>();

        // Servicio para el zip
        services.AddScoped<IZipStreamingService, ZipStreamingService>();

        // Token para enlace temporal
        services.AddScoped<ITokenSeguroGenerator, TokenSeguroGenerator>();

        // Repositories
        services.Scan(scan => scan
        .FromAssemblyOf<AreaRepository>()
        .AddClasses(classes => classes
        .Where(type => type.Name.EndsWith("Repository") && !type.IsAbstract))
        .AsImplementedInterfaces()
        .WithScopedLifetime());

        return services;
    }

    public static IServiceCollection AddWorkerInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<SucWcfOptions>(configuration.GetSection(SucWcfOptions.SectionName));

        services.AddSingleton<ISucConfigurationClient, SucConfigurationClient>();

        services.AddSingleton<IConfigurationCache, ConfigurationCache>();

        services.AddSingleton<IRuntimeConfigurationProvider, RuntimeConfigurationProvider>();

        services.AddMemoryCache();

        services.AddDbContext<WorkflowFirmaDbContext>((sp, options) =>
        {
            var runtime =
                sp.GetRequiredService<IRuntimeConfigurationProvider>();

            var connection =
                runtime
                    .GetOracleConnectionAsync()
                    .GetAwaiter()
                    .GetResult();

            options.UseOracle(connection,
                oracle =>
                {
                    oracle.CommandTimeout(60);
                });
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // SOLO lo que usa el worker
        services.AddScoped<IParametroRepository, ParametroRepository>();

        return services;
    }
}
