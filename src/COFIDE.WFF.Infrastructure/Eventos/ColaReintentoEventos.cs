using System.Threading.Channels;
using COFIDE.WFF.Domain.Common.Abstractions;
using Microsoft.Extensions.Options;

namespace COFIDE.WFF.Infrastructure.Eventos;

public sealed record EventoPendiente(IDomainEvent Evento, int Intento, DateTime ReintentarDesde);

/// <summary>
/// Cola EN MEMORIA de eventos cuya publicación inmediata falló. No usa tablas: si la Api se reinicia,
/// los eventos en espera se pierden (quedan registrados en el log). Se reemplazará por una cola de mensajes.
/// </summary>
public sealed class ColaReintentoEventos(IOptions<PublicacionEventosOptions> options)
{
    private readonly Channel<EventoPendiente> _canal = Channel.CreateBounded<EventoPendiente>(
        new BoundedChannelOptions(Math.Max(1, options.Value.CapacidadCola))
        {
            FullMode = BoundedChannelFullMode.DropWrite,
            SingleReader = true
        });

    public ChannelReader<EventoPendiente> Lector => _canal.Reader;

    /// <summary>Devuelve false si la cola está llena (el evento se descarta y debe registrarse en el log).</summary>
    public bool Encolar(EventoPendiente pendiente) => _canal.Writer.TryWrite(pendiente);
}
