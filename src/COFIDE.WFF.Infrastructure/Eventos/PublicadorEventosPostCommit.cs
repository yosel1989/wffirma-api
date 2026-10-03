using COFIDE.WFF.Application.Common.Events;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Domain.Common.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace COFIDE.WFF.Infrastructure.Eventos;

/// <summary>
/// Publica cada evento post-commit de inmediato, dentro de la misma petición (como antes del cambio).
/// Si un evento falla, se envía a la cola de reintentos en segundo plano y la petición continúa sin error.
/// </summary>
public sealed class PublicadorEventosPostCommit(
        IDomainEventDispatcher dispatcher,
        ColaReintentoEventos cola,
        IOptions<PublicacionEventosOptions> options,
        ILogger<PublicadorEventosPostCommit> logger
    ) : IPublicadorEventosPostCommit
{
    private readonly IDomainEventDispatcher _dispatcher = dispatcher;
    private readonly ColaReintentoEventos _cola = cola;
    private readonly PublicacionEventosOptions _options = options.Value;
    private readonly ILogger<PublicadorEventosPostCommit> _logger = logger;

    public async Task PublicarAsync(IReadOnlyCollection<IDomainEvent> eventos, CancellationToken cancellationToken)
    {
        // Uno por uno: si un evento falla, los demás igual se publican.
        foreach (var evento in eventos)
        {
            try
            {
                await _dispatcher.DispatchAsync([evento], cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falló la publicación inmediata de {Evento}. Se reintentará en segundo plano.", evento);
                EncolarReintento(evento, intento: 1);
            }
        }
    }

    private void EncolarReintento(IDomainEvent evento, int intento)
    {
        if (_options.MaximoReintentos <= 0)
        {
            _logger.LogError("Evento no publicado y sin reintentos configurados: {Evento}", evento);
            return;
        }

        var pendiente = new EventoPendiente(evento, intento, DateTime.UtcNow.AddSeconds(_options.SegundosEntreReintentos * intento));
        if (!_cola.Encolar(pendiente))
            _logger.LogError("Cola de reintentos llena. Evento no publicado: {Evento}", evento);
    }
}
