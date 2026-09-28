using COFIDE.WFF.Application.Features.Jobs.Services;
using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
namespace COFIDE.WFF.Worker.BackgroundJobs;

public class ActualizarEstadoProgramadoWorker(
        ILogger<ActualizarEstadoProgramadoWorker> logger,
        IServiceProvider serviceProvider
    ) : BackgroundService
{
    private readonly ILogger<ActualizarEstadoProgramadoWorker> _logger = logger;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();

                var parametroRepo = scope.ServiceProvider.GetRequiredService<IParametroRepository>();
                var jobService = scope.ServiceProvider.GetRequiredService<IActualizarEstadoProgramadoService>();

                var config = await parametroRepo.ObtenerConfiguracionAsync(stoppingToken);
                var obtenerDetalleJOB = await parametroRepo.ObtenerCodigoParametroDias("IN_ACTIVA_JOB", stoppingToken);

                if (config is null)
                {
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                    continue;
                }

                var horaInicio = TimeSpan.FromHours(config.HoraInicio);
                var intervalo = TimeSpan.FromMinutes(config.FrecuenciaMinutos);

                var ahora = DateTime.Now;
                var proxima = CalcularProximaEjecucion(ahora, horaInicio, intervalo);
                var espera = proxima - ahora;

                _logger.LogInformation("Próxima ejecución: {Proxima}", proxima);

                await Task.Delay(espera, stoppingToken);

                if (obtenerDetalleJOB.Count > 0)
                {
                    var flagJob = obtenerDetalleJOB.FirstOrDefault()?.Valor;
                    string valorConfig = string.IsNullOrWhiteSpace(flagJob) ? "1" : flagJob;

                    if (valorConfig == "0")
                    {
                        _logger.LogInformation("La ejecución del job se encuentra deshabilitada por configuración.");
                        continue;
                    }
                }

                var actualizados = await jobService.EjecutarAsync(stoppingToken);

                _logger.LogInformation("Registros actualizados: {Cantidad}", actualizados);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Worker detenido por cancelación de tarea.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el worker");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }

    private static DateTime CalcularProximaEjecucion(DateTime ahora, TimeSpan horaInicio, TimeSpan intervalo)
    {
        var primera = ahora.Date.Add(horaInicio);

        if (ahora < primera)
            return primera;

        var transcurrido = ahora - primera;
        var bloques = Math.Ceiling(transcurrido.TotalMinutes / intervalo.TotalMinutes);

        return primera.AddMinutes(bloques * intervalo.TotalMinutes);
    }
}