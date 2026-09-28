using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Infrastructure.Providers;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
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

            var apiKey = await _hubApiKeyProvider.ObtenerApiKeyAsync(cancellationToken);


            _httpClient.DefaultRequestHeaders.Remove("X-Api-Key");
            _httpClient.DefaultRequestHeaders.Add("X-Api-Key", apiKey.HubApiKey);

            var response = await _httpClient.PostAsJsonAsync(
                "NotificacionesHub/firma-documento",
                payload,
                cancellationToken);

            response.EnsureSuccessStatusCode();

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
}
