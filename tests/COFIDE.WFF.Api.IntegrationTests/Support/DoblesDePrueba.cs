using System.Collections.Concurrent;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;

namespace COFIDE.WFF.Api.IntegrationTests.Support;

/// <summary>Registra los correos en memoria; nunca llama a la API de correo.</summary>
public sealed class CorreoDePrueba : ICorreoSenderPort
{
    private readonly ConcurrentQueue<CorreoEnviado> _enviados = new();

    public IReadOnlyCollection<CorreoEnviado> Enviados => _enviados.ToArray();

    public Task EnviarCorreoAsync(string asunto, string cuerpo, string destinatario, string remitente, CancellationToken cancellationToken = default)
    {
        _enviados.Enqueue(new CorreoEnviado(asunto, destinatario, remitente));
        return Task.CompletedTask;
    }
}

public sealed record CorreoEnviado(string Asunto, string Destinatario, string Remitente);

/// <summary>Ignora las notificaciones en tiempo real (hub de sockets) durante las pruebas.</summary>
public sealed class NotificacionTiempoRealDePrueba : INotificacionTiempoRealPort
{
    public Task NotificarFirmaDocumentoAsync(long solicitudId, long solicitudDocumentoId, string estadoSolicitud,
        bool firmado, string codigoUsuario, string mensaje, CancellationToken cancellationToken) =>
        Task.CompletedTask;
}
