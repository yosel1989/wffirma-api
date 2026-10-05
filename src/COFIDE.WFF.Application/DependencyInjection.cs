using COFIDE.WFF.Application.Common.Behaviors;
using COFIDE.WFF.Application.Common.Events;
using COFIDE.WFF.Application.Features.Firmas.Services;
using COFIDE.WFF.Application.Features.Firmas.UseCases;
using COFIDE.WFF.Application.Features.Jobs.Services;
using COFIDE.WFF.Application.Features.Plantillas.Services;
using COFIDE.WFF.Application.Features.Solicitudes.Services;
using COFIDE.WFF.Domain.DomainServices;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace COFIDE.WFF.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        services.AddAutoMapper(cfg => { }, typeof(DependencyInjection).Assembly);

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditingBehavior<,>));

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthenticatedUserBehavior<,>));

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));


        services.AddScoped<ISolicitudDescargaZipService, SolicitudDescargaZipService>();
        services.AddScoped<ISolicitudAnulacionService, SolicitudAnulacionService>();
        services.AddScoped<IPlantillaCreacionService, PlantillaCreacionService>();
        services.AddScoped<IWorkflowDomainService, WorkflowDomainService>();

        services.AddScoped<IFlujoDiffDomainService, FlujoDiffDomainService>();
        // Dispatcher para Domain Events
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();


        //Nuevo
        services.AddScoped<ISolicitudCreacionService, SolicitudCreacionService>();
        services.AddScoped<ISolicitudDocumentoService, SolicitudDocumentoService>();
        services.AddScoped<ISolicitudSustentoService, SolicitudSustentoService>();
        services.AddScoped<ISolicitudPlantillaService, SolicitudPlantillaService>();


        // Use Case
        services.Scan(scan => scan
           .FromAssemblyOf<FirmaInternaPdfUseCase>()
           .AddClasses(classes => classes
           .Where(type => type.Name.EndsWith("UseCase") && !type.IsAbstract))
           .AsImplementedInterfaces()
           .WithScopedLifetime());

        // Services
        services.Scan(scan => scan
          .FromAssemblyOf<FirmaPreparationService>()
          .AddClasses(classes => classes
          .Where(type => type.Name.EndsWith("Service") && !type.IsAbstract))
          .AsImplementedInterfaces()
          .WithScopedLifetime());

        return services;
    }
}
