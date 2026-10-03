using COFIDE.WFF.Application.Common.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace COFIDE.WFF.Infrastructure.Eventos;

/// <summary>
/// Reintenta en segundo plano los eventos cuya publicación inmediata falló.
/// Si se agotan los reintentos, el evento queda registrado en el log (nivel Error) para revisión manual.
/// </summary>
public sealed class ReintentoEventosService(
        ColaReintentoEventos cola,
        IServiceScopeFactory scopeFactory,
        IOptions<PublicacionEventosOptions> options,
        ILogger<ReintentoEventosService> logger
    ) : BackgroundService
{
    private readonly ColaReintentoEventos _cola = cola;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly PublicacionEventosOptions _options = options.Value;
    private readonly ILogger<ReintentoEventosService> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var pendiente in _cola.Lector.ReadAllAsync(stoppingToken))
                await ProcesarAsync(pendiente, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Apagado de la aplicación.
        }
    }

    public async Task ProcesarAsync(EventoPendiente pendiente, CancellationToken cancellationToken)
    {
        var espera = pendiente.ReintentarDesde - DateTime.UtcNow;
        if (espera > TimeSpan.Zero)
            await Task.Delay(espera, cancellationToken);

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var dispatcher = scope.ServiceProvider.GetRequiredService<IDomainEventDispatcher>();
            await dispatcher.DispatchAsync([pendiente.Evento], cancellationToken);

            _logger.LogInformation("Evento publicado en el reintento {Intento}: {Evento}", pendiente.Intento, pendiente.Evento);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            if (pendiente.Intento >= _options.MaximoReintentos)
            {
                _logger.LogError(ex, "Evento NO publicado tras {Intentos} reintentos: {Evento}", pendiente.Intento, pendiente.Evento);
                return;
            }

            var siguiente = pendiente with
            {
                Intento = pendiente.Intento + 1,
                ReintentarDesde = DateTime.UtcNow.AddSeconds(_options.SegundosEntreReintentos * (pendiente.Intento + 1))
            };

            _logger.LogWarning(ex, "Falló el reintento {Intento} de {Evento}.", pendiente.Intento, pendiente.Evento);

            if (!_cola.Encolar(siguiente))
                _logger.LogError("Cola de reintentos llena. Evento no publicado: {Evento}", pendiente.Evento);
        }
    }
}
