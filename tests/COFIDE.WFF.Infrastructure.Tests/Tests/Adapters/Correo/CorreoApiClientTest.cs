using System.Net;
using System.Text;
using System.Text.Json;
using COFIDE.WFF.Infrastructure.Adapters.ExternalServices.Correo;
using COFIDE.WFF.Infrastructure.Adapters.ExternalServices.Correo.DTOs;
using COFIDE.WFF.Infrastructure.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace COFIDE.WFF.Infrastructure.Tests.Tests.Adapters.Correo;

/// <summary>Envía el correo contra un HTTP de prueba y lee el JSON que saldría hacia Api Correo.</summary>
public class CorreoApiClientTest
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Debe_Publicar_El_Correo_Con_Los_Destinatarios_Limpios()
    {
        using var sut = Sujeto.Ok();

        await sut.Cliente.EnviarCorreoAsync(
            "Contrato",
            "<p>Revisar</p>",
            " ana@cofide.com.pe ;\n luis@cofide.com.pe ; ;",
            "firmas@cofide.com.pe");

        sut.Api.Solicitud!.Method.Should().Be(HttpMethod.Post);
        sut.Api.Solicitud.RequestUri!.AbsolutePath.Should().Be("/api/v1/Correo/enviocorreo");

        var cuerpo = JsonSerializer.Deserialize<EnviarCorreoRequestDto>(sut.Api.Cuerpo!, Json);
        cuerpo!.NombreUsuario.Should().Be("Workflow Firmas");
        cuerpo.Sistema.Should().Be("WFF");
        cuerpo.IpEquipo.Should().Be("::1");
        cuerpo.Asunto.Should().Be("Contrato");
        cuerpo.Cuerpo.Should().Be("<p>Revisar</p>");
        cuerpo.Remitente.Should().Be("firmas@cofide.com.pe");
        cuerpo.Para.Should().Be("ana@cofide.com.pe;luis@cofide.com.pe");
        cuerpo.Resultado.Should().Be("??");
        cuerpo.Fecha.Should().MatchRegex(@"^\d{4}-\d{2}-\d{2}$");
        cuerpo.Hora.Should().MatchRegex(@"^\d{2}:\d{2}:\d{2}$");
    }

    [Fact]
    public async Task Codigo_Distinto_De_Cero_Debe_Fallar_Con_El_Mensaje_De_La_Api()
    {
        using var sut = Sujeto.Json(HttpStatusCode.OK, """{"code":12,"message":"No se pudo enviar"}""");

        var act = () => sut.Cliente.EnviarCorreoAsync("a", "b", "ana@cofide.com.pe", "firmas@cofide.com.pe");

        var error = await act.Should().ThrowAsync<InfrastructureException>();
        error.Which.Error.Code.Should().Be("API_CORREO_LOGIC_12");
        error.Which.Error.Description.Should().Be("No se pudo enviar");
        error.Which.HttpStatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    [Fact]
    public async Task Respuesta_Vacia_Debe_Fallar()
    {
        using var sut = Sujeto.Json(HttpStatusCode.OK, "null");

        var act = () => sut.Cliente.EnviarCorreoAsync("a", "b", "ana@cofide.com.pe", "firmas@cofide.com.pe");

        var error = await act.Should().ThrowAsync<InfrastructureException>();
        error.Which.Error.Code.Should().Be(CorreoErrors.EmptyResponse.Code);
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, "API_CORREO_401", 401)]
    [InlineData(HttpStatusCode.Forbidden, "API_CORREO_403", 403)]
    [InlineData(HttpStatusCode.NotFound, "API_CORREO_404", 404)]
    [InlineData(HttpStatusCode.InternalServerError, "API_CORREO_500", 502)]
    public async Task Error_Http_Debe_Mapearse(HttpStatusCode estado, string codigo, int estadoEsperado)
    {
        using var sut = Sujeto.Json(estado, """{"detalle":"no"}""");

        var act = () => sut.Cliente.EnviarCorreoAsync("a", "b", "ana@cofide.com.pe", "firmas@cofide.com.pe");

        var error = await act.Should().ThrowAsync<InfrastructureException>();
        error.Which.Error.Code.Should().Be(codigo);
        error.Which.HttpStatusCode.Should().Be(estadoEsperado);
    }

    [Fact]
    public async Task Error_Http_No_Mapeado_Debe_Incluir_El_Detalle()
    {
        using var sut = Sujeto.Texto(HttpStatusCode.BadRequest, "servicio caido");

        var act = () => sut.Cliente.EnviarCorreoAsync("a", "b", "ana@cofide.com.pe", "firmas@cofide.com.pe");

        var error = await act.Should().ThrowAsync<InfrastructureException>();
        error.Which.Error.Code.Should().Be(CorreoErrors.HttpError.Code);
        error.Which.Error.Description.Should().Contain("servicio caido");
        error.Which.HttpStatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Cancelacion_Debe_Fallar_Por_Timeout()
    {
        using var sut = Sujeto.Falla(new TaskCanceledException("cancelado"));

        var act = () => sut.Cliente.EnviarCorreoAsync("a", "b", "ana@cofide.com.pe", "firmas@cofide.com.pe");

        var error = await act.Should().ThrowAsync<InfrastructureException>();
        error.Which.Error.Code.Should().Be(CorreoErrors.Timeout.Code);
        error.Which.HttpStatusCode.Should().Be((int)HttpStatusCode.GatewayTimeout);
        error.Which.InnerException.Should().BeOfType<TaskCanceledException>();
    }

    [Fact]
    public async Task Error_De_Conexion_Debe_Fallar()
    {
        using var sut = Sujeto.Falla(new HttpRequestException("sin red"));

        var act = () => sut.Cliente.EnviarCorreoAsync("a", "b", "ana@cofide.com.pe", "firmas@cofide.com.pe");

        var error = await act.Should().ThrowAsync<InfrastructureException>();
        error.Which.Error.Code.Should().Be(CorreoErrors.ConnectionError.Code);
        error.Which.HttpStatusCode.Should().Be((int)HttpStatusCode.BadGateway);
        error.Which.InnerException.Should().BeOfType<HttpRequestException>();
    }

    private sealed class Sujeto : IDisposable
    {
        private readonly HttpClient _http;
        public CorreoApiClient Cliente { get; }
        public ApiCorreoDePrueba Api { get; }

        private Sujeto(ApiCorreoDePrueba api)
        {
            Api = api;
            _http = new HttpClient(api) { BaseAddress = new Uri("https://correo.test/") };
            Cliente = new CorreoApiClient(
                _http,
                Options.Create(new CorreoApiOptions { BaseUrl = "https://correo.test/", CodigoSistema = "WFF" }),
                NullLogger<CorreoApiClient>.Instance);
        }

        public static Sujeto Ok() => new(new ApiCorreoDePrueba(_ => Respuesta(HttpStatusCode.OK, """{"code":0,"message":"ok"}""")));

        public static Sujeto Json(HttpStatusCode estado, string cuerpo) =>
            new(new ApiCorreoDePrueba(_ => Respuesta(estado, cuerpo)));

        public static Sujeto Texto(HttpStatusCode estado, string cuerpo) =>
            new(new ApiCorreoDePrueba(_ => new HttpResponseMessage(estado)
            {
                Content = new StringContent(cuerpo, Encoding.UTF8, "text/plain")
            }));

        public static Sujeto Falla(Exception error) =>
            new(new ApiCorreoDePrueba(_ => throw error));

        private static HttpResponseMessage Respuesta(HttpStatusCode estado, string cuerpo) =>
            new(estado)
            {
                Content = new StringContent(cuerpo, Encoding.UTF8, "application/json")
            };

        public void Dispose() => _http.Dispose();
    }

    private sealed class ApiCorreoDePrueba(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        public HttpRequestMessage? Solicitud { get; private set; }
        public string? Cuerpo { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Solicitud = request;
            if (request.Content is not null)
                Cuerpo = await request.Content.ReadAsStringAsync(cancellationToken);

            return responder(request);
        }
    }
}
