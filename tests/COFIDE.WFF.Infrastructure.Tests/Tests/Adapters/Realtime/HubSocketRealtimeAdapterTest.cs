using System.Net;
using System.Text.Json;
using COFIDE.WFF.Infrastructure.Adapters.ExternalServices.Realtime;
using COFIDE.WFF.Infrastructure.Bootstrap;
using COFIDE.WFF.Infrastructure.Providers;
using Microsoft.Extensions.Logging.Abstractions;

namespace COFIDE.WFF.Infrastructure.Tests.Tests.Adapters.Realtime;

public class HubSocketRealtimeAdapterTest
{
    [Theory]
    [InlineData(true, "/NotificacionesHub/enviar-usuario", 0L, "Tiene una solicitud pendiente por firmar o aprobar")]
    [InlineData(false, "/NotificacionesHub/firma-documento", 19L, "")]
    public async Task EnviaCadaAvisoConSuRutaYCuerpo(bool esSolicitudPendiente, string ruta, long documentoId, string mensaje)
    {
        using var http = new HubDePrueba();
        using var cliente = new HttpClient(http) { BaseAddress = new Uri("https://hub.example/") };
        var adaptador = new HubSocketRealtimeAdapter(cliente, new ApiKeyDePrueba(), NullLogger<HubSocketRealtimeAdapter>.Instance);

        if (esSolicitudPendiente)
            await adaptador.NotificarSolicitudPendienteFirma(42, 7, new List<string> { "JPerez", "MLoPEZ" }, mensaje, default);
        else
            await adaptador.NotificarFirmaDocumentoAsync(42, documentoId, "2", true, "jperez", mensaje, default);

        Assert.Equal(HttpMethod.Post, http.Metodo);
        Assert.Equal(ruta, http.Ruta);
        Assert.Equal("clave-de-prueba", http.ApiKey);
        using var json = JsonDocument.Parse(Assert.IsType<string>(http.Cuerpo));
        var cuerpo = json.RootElement;

        if (esSolicitudPendiente)
        {
            Assert.Equal(4, cuerpo.EnumerateObject().Count());
            Assert.Equal(new[] { "jperez", "mlopez" }, cuerpo.GetProperty("idUsuario").EnumerateArray().Select(usuario => usuario.GetString()));
            Assert.Equal("notificacion-solicitud-pendiente", cuerpo.GetProperty("eventName").GetString());
            Assert.Equal(mensaje, cuerpo.GetProperty("message").GetString());
            var data = cuerpo.GetProperty("data");
            Assert.Equal(42L, data.GetProperty("solicitudId").GetInt64());
            Assert.Equal(7, data.GetProperty("totalSolicitudesPendientesFirma").GetInt32());
        }
        else
        {
            Assert.Equal(42L, cuerpo.GetProperty("solicitudId").GetInt64());
            Assert.Equal("jperez", cuerpo.GetProperty("codigoUsuario").GetString());
            Assert.Equal(mensaje, cuerpo.GetProperty("mensaje").GetString());
            Assert.Equal(6, cuerpo.EnumerateObject().Count());
            Assert.Equal(documentoId, cuerpo.GetProperty("solicitudDocumentoId").GetInt64());
            Assert.True(cuerpo.GetProperty("firmado").GetBoolean());
            Assert.Equal("2", cuerpo.GetProperty("estadoSolicitud").GetString());
        }
    }

    private sealed class ApiKeyDePrueba : IHubApiKeyProvider
    {
        public Task<SecurityConfiguration> ObtenerApiKeyAsync(CancellationToken cancellationToken) =>
            Task.FromResult(new SecurityConfiguration { HubApiKey = "clave-de-prueba" });
    }

    private sealed class HubDePrueba : HttpMessageHandler
    {
        public HttpMethod? Metodo { get; private set; }
        public string? Ruta { get; private set; }
        public string? ApiKey { get; private set; }
        public string? Cuerpo { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Metodo = request.Method;
            Ruta = request.RequestUri?.AbsolutePath;
            ApiKey = request.Headers.TryGetValues("X-Api-Key", out var valores) ? valores.Single() : null;
            Cuerpo = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }
    }
}
