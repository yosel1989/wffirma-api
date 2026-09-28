using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Jobs.Services;
using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Infrastructure;
using COFIDE.WFF.Infrastructure.Adapters.Repositories;
using COFIDE.WFF.Worker.BackgroundJobs;
using COFIDE.WFF.Worker.Logging;
using COFIDE.WFF.Worker.Services;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWindowsService();
SerilogConfigurator.Configure(builder, "COFIDE.WFF.WORKER");


builder.Services.AddWorkerInfrastructureServices(builder.Configuration);
builder.Services.AddScoped<ICurrentUserService, WorkerCurrentUserService>();
builder.Services.AddScoped<IActualizarEstadoProgramadoService, ActualizarEstadoProgramadoService>();
builder.Services.AddHostedService<ActualizarEstadoProgramadoWorker>();

builder.Services.AddScoped<IParametroRepository, ParametroRepository>();
builder.Services.AddScoped<IDetalleFirmaRepository, DetalleFirmaRepository>();


var host = builder.Build();

try
{
    Log.Information("Worker iniciado correctamente");
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Error fatal en el worker");
}
finally
{
    Log.CloseAndFlush();
}