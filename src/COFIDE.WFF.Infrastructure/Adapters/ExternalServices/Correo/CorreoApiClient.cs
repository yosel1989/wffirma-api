using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Infrastructure.Adapters.ExternalServices.Correo.DTOs;
using COFIDE.WFF.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Json;
namespace COFIDE.WFF.Infrastructure.Adapters.ExternalServices.Correo;

public class CorreoApiClient(
        HttpClient httpClient,
        IOptions<CorreoApiOptions> options,
        ILogger<CorreoApiClient> logger
    ) : ICorreoSenderPort
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly CorreoApiOptions _options = options.Value;
    private readonly ILogger<CorreoApiClient> _logger = logger;
    private readonly string senderAlias = "COFIDE Workflow de Firmas";

    public async Task EnviarCorreoAsync(
        string asunto,
        string cuerpo,
        string destinatario,
        string remitente, CancellationToken cancellationToken = default)
    {
        var destinatariosFormateados = FormatearDestinatarios(destinatario);

        var request = BuildRequest(asunto, cuerpo, destinatariosFormateados, remitente);

        try
        {
            _logger.LogInformation(
                "Enviando correo a API_CORREO. Destinatarios: {Destinatarios}",
                destinatariosFormateados);

            var url = $"api/v1/Correo/enviocorreo";

            var response = await _httpClient
                .PostAsJsonAsync(url, request, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                throw await MapHttpError(response);

            var dto = await response.Content
             .ReadFromJsonAsync<EnviarCorreoResponseDto>(cancellationToken: cancellationToken)
                ?? throw new InfrastructureException(CorreoErrors.EmptyResponse);

            if (dto.Code != 0)
                throw new InfrastructureException(
                    CorreoErrors.LogicalError(dto.Code.ToString(), dto.Message),
                    (int)response.StatusCode);
        }
        catch (TaskCanceledException ex)
        {
            throw new InfrastructureException(
                CorreoErrors.Timeout,
                (int)HttpStatusCode.GatewayTimeout,
                ex);
        }
        catch (HttpRequestException ex)
        {
            throw new InfrastructureException(
                CorreoErrors.ConnectionError,
                (int)HttpStatusCode.BadGateway,
                ex);
        }
    }
    private static async Task<InfrastructureException> MapHttpError(
           HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        return response.StatusCode switch
        {
            HttpStatusCode.Unauthorized =>
                new InfrastructureException(
                    CorreoErrors.Unauthorized,
                    (int)HttpStatusCode.Unauthorized),

            HttpStatusCode.Forbidden =>
                new InfrastructureException(
                    CorreoErrors.Forbidden,
                    (int)HttpStatusCode.Forbidden),

            HttpStatusCode.NotFound =>
                new InfrastructureException(
                    CorreoErrors.NotFound,
                    (int)HttpStatusCode.NotFound),

            HttpStatusCode.InternalServerError =>
                new InfrastructureException(
                    CorreoErrors.InternalServerError,
                    (int)HttpStatusCode.BadGateway),

            _ =>
                new InfrastructureException(
                    CorreoErrors.HttpErrorWithDetail(content),
                    (int)response.StatusCode)
        };
    }

    private static string FormatearDestinatarios(string destinatarios)
    {
        return string.Join(";",
            destinatarios
                .Replace("\r", "")
                .Replace("\n", "")
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x)));
    }

    private EnviarCorreoRequestDto BuildRequest(
        string asunto,
        string cuerpo,
        string destinatarios,
        string remitente)
    {
        var now = DateTime.UtcNow;

        return new EnviarCorreoRequestDto
        {
            NombreUsuario = "Workflow Firmas",
            Sistema = _options.CodigoSistema,
            Fecha = now.ToString("yyyy-MM-dd"),
            Hora = now.ToString("HH:mm:ss"),
            IpEquipo = "::1",
            Asunto = asunto,
            Cuerpo = cuerpo,
            Remitente = remitente,
            AliasRemitente = senderAlias,
            Para = destinatarios,
            Resultado = "??"
        };
    }
}
