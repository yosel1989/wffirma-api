using COFIDE.WFF.Application.Common.Constants;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Infrastructure.Exceptions;
using COFIDE.WFF.Infrastructure.Providers;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
namespace COFIDE.WFF.Infrastructure.Adapters.ExternalServices.Realtime;

public class HubSocketRealtimeAdapter(
        HttpClient httpClient,
        IHubApiKeyProvider hubApiKeyProvider,
        ILogger<HubSocketRealtimeAdapter> logger
    ) : INotificacionTiempoRealPort
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly IHubApiKeyProvider _hubApiKeyProvider = hubApiKeyProvider;
    private readonly ILogger<HubSocketRealtimeAdapter> _logger = logger;

    public async Task NotificarFirmaDocumentoAsync(
        long solicitudId,
        long solicitudDocumentoId,
        string estadoSolicitud,
        bool firmado,
        string codigoUsuario,
        string mensaje,
        CancellationToken cancellationToken)
    {
        try
        {
            var payload = new
            {
                solicitudId,
                solicitudDocumentoId,
                firmado,
                estadoSolicitud,
                codigoUsuario,
                mensaje
            };

            await EnviarAsync("NotificacionesHub/firma-documento", payload, cancellationToken);

            _logger.LogInformation(
                "Notificación realtime enviada Solicitud={SolicitudId}, Documento={DocumentoId}",
                solicitudId,
                solicitudDocumentoId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en HubSocket");
        }
    }

    /// <inheritdoc />
    public async Task NotificarSolicitudPendienteFirma(
        long solicitudId,
        int totalSolicitudesPendientesFirma,
        List<string> idUsuario,
        string mensaje,
        CancellationToken cancellationToken)
    {
        var payload = new
        {
            idUsuario = idUsuario.Select(usuario => usuario.ToLowerInvariant()).ToList(),
            eventName = EventosNotificacion.SolicitudPendiente,
            message = mensaje,
            data = JsonSerializer.SerializeToElement(new
            {
                solicitudId,
                totalSolicitudesPendientesFirma
            })
        };

        try
        {
            await EnviarAsync("NotificacionesHub/enviar-usuario", payload, cancellationToken);
            _logger.LogInformation("Aviso de solicitud pendiente de firma enviado. Solicitud={SolicitudId}", solicitudId);
        }
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException
            or InfrastructureException or InvalidOperationException or JsonException or FormatException)
        {
            _logger.LogError(ex, "Error en HubSocket al avisar de una solicitud pendiente de firma. Solicitud={SolicitudId}", solicitudId);
        }
    }

    private async Task EnviarAsync<T>(string ruta, T payload, CancellationToken cancellationToken)
    {
        var apiKey = await _hubApiKeyProvider.ObtenerApiKeyAsync(cancellationToken);
        using var request = new HttpRequestMessage(HttpMethod.Post, ruta)
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Add("X-Api-Key", apiKey.HubApiKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
