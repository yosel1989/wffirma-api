using System.Globalization;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace COFIDE.WFF.Api.IntegrationTests.Tests.ApisExternas;

/// <summary>Comprueba la respuesta HTTP del endpoint existente del hub en desarrollo.</summary>
/// <remarks>Esta prueba envia una notificacion real; solo debe ejecutarse con datos de prueba autorizados.</remarks>
[Trait("Categoria", "ApisExternas")]
public sealed class HubSocketRespuestaTest(ITestOutputHelper output)
{
    /// <summary>Envia la misma estructura que el adaptador y exige una respuesta HTTP exitosa.</summary>
    [FactConHubSocket]
    public async Task FirmaDocumento_Responde_Con_Exito()
    {
        var apiKey = RequerirVariable("WFF_IT_HUB_API_KEY");
        var codigoUsuario = RequerirVariable("WFF_IT_HUB_CODIGO_USUARIO");
        var solicitudId = LeerId("WFF_IT_HUB_SOLICITUD_ID");
        var documentoId = LeerId("WFF_IT_HUB_DOCUMENTO_ID");
        var estadoSolicitud = RequerirVariable("WFF_IT_HUB_ESTADO_SOLICITUD");

        using var handler = new HttpClientHandler { AllowAutoRedirect = false };
        using var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://skhubdev.cofide.com.pe/"),
            Timeout = TimeSpan.FromSeconds(15)
        };
        using var request = new HttpRequestMessage(HttpMethod.Post, "NotificacionesHub/firma-documento")
        {
            Content = JsonContent.Create(new
            {
                solicitudId,
                solicitudDocumentoId = documentoId,
                firmado = true,
                estadoSolicitud,
                codigoUsuario,
                mensaje = "Prueba de integracion autorizada"
            })
        };
        request.Headers.Add("X-Api-Key", apiKey);

        using var response = await client.SendAsync(request);
        output.WriteLine("Estado HTTP: {0}", (int)response.StatusCode);
        output.WriteLine("Tipo de contenido: {0}", response.Content.Headers.ContentType?.MediaType ?? "no indicado");

        Assert.True(response.IsSuccessStatusCode,
            $"El servicio de desarrollo respondio HTTP {(int)response.StatusCode}; se esperaba 2xx.");
    }

    private static string RequerirVariable(string nombre)
    {
        var valor = Environment.GetEnvironmentVariable(nombre);
        if (string.IsNullOrWhiteSpace(valor))
            throw new InvalidOperationException($"Debe definir la variable de entorno {nombre} para esta prueba.");
        return valor;
    }

    private static long LeerId(string nombre)
    {
        var valor = RequerirVariable(nombre);
        if (!long.TryParse(valor, NumberStyles.None, CultureInfo.InvariantCulture, out var id) || id <= 0)
            throw new InvalidOperationException($"La variable de entorno {nombre} debe contener un identificador entero positivo.");
        return id;
    }
}

/// <summary>Habilita explicitamente la prueba que envia una notificacion real al hub de desarrollo.</summary>
public sealed class FactConHubSocketAttribute : FactAttribute
{
    /// <summary>Omite la prueba salvo que el usuario autorice su ejecucion en el proceso actual.</summary>
    public FactConHubSocketAttribute()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("WFF_IT_HUB_HABILITADO"), "true", StringComparison.OrdinalIgnoreCase))
            Skip = "La prueba envia una notificacion real. Active WFF_IT_HUB_HABILITADO=true solo con datos de prueba autorizados.";
    }
}
