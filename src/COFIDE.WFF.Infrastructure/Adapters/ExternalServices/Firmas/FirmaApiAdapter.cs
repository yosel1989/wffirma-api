using COFIDE.WFF.Application.Features.Firmas.DTOs;
using COFIDE.WFF.Application.Features.Firmas.Ports.Out;
using COFIDE.WFF.Infrastructure.Adapters.ExternalServices.Firmas.DTOs;
using COFIDE.WFF.Infrastructure.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
namespace COFIDE.WFF.Infrastructure.Adapters.ExternalServices.Firmas;

public class FirmaApiAdapter(
        HttpClient httpClient,
        IOptions<FirmaApiOptions> options
    ) : IFirmaPort
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly FirmaApiOptions _options = options.Value;

    public async Task<FirmaInternaResponseDto> FirmarDocumentoAsync(
            FirmarDocumentoRequestDto request,
            CancellationToken cancellationToken)
    {
        try
        {
            var url = $"api/v1/firmas/firmar";

            using var formData = BuildRequestAsync(request, true, false);

            var response = await _httpClient.PostAsync(url, formData, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                throw await MapHttpError(response);

            var wrapper = await response.Content
              .ReadFromJsonAsync<FirmaResponseDto<FirmaInternaResponseDto>>(cancellationToken: cancellationToken)
              ?? throw new InfrastructureException(FirmaErrors.FIRD_EmptyResponse);

            if (!wrapper.Success)
                throw new InfrastructureException(
                    FirmaErrors.FIRD_LogicalError(wrapper.Code, wrapper.Message),
                    (int)response.StatusCode);

            if (wrapper.Data is null)
                throw new InfrastructureException(FirmaErrors.FIRD_EmptyResponse);

            return wrapper.Data;
        }
        catch (TaskCanceledException ex)
        {
            throw new InfrastructureException(
                FirmaErrors.FIRD_Timeout,
                (int)HttpStatusCode.GatewayTimeout,
                ex);
        }
        catch (HttpRequestException ex)
        {          
            throw new InfrastructureException(
                FirmaErrors.FIRD_ConnectionError,
                (int)HttpStatusCode.BadGateway,
                ex);
        }
    }

    public async Task<FirmaInternaResponseDto> FirmarBatchAsync(
            FirmarDocumentoRequestDto request,
            CancellationToken cancellationToken)
    {

        try
        {
            var url = $"api/v1/firmas/firmar-batch";

            using var formData = BuildRequestAsync(request, false, true);

            var response = await _httpClient.PostAsync(url, formData, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                throw await MapHttpError(response);

            var wrapper = await response.Content
              .ReadFromJsonAsync<FirmaResponseDto<FirmaInternaResponseDto>>(cancellationToken: cancellationToken)
                ?? throw new InfrastructureException(FirmaErrors.FIRD_EmptyResponse);

            if (!wrapper.Success)
                throw new InfrastructureException(
                    FirmaErrors.FIRD_LogicalError(wrapper.Code, wrapper.Message),
                    (int)response.StatusCode);

            if (wrapper.Data is null)
                throw new InfrastructureException(FirmaErrors.FIRD_EmptyResponse);

            return wrapper.Data;

        }
        catch (TaskCanceledException ex)
        {
            throw new InfrastructureException(
                FirmaErrors.FIRD_Timeout,
                (int)HttpStatusCode.GatewayTimeout,
                ex);
        }
        catch (HttpRequestException ex)
        {
            throw new InfrastructureException(
                FirmaErrors.FIRD_ConnectionError,
                (int)HttpStatusCode.BadGateway,
                ex);
        }

    }

    public async Task<byte[]> ObtenerDocumentoFirmadoAsync(
           string codigoUsuario, string codigo, string documentId,
            CancellationToken cancellationToken)
    {
        try
        {          
            var url = $"api/v1/firmas/obtener-documento-firmado";

            var request = new DocumentoFirmadoRequestDto
            {
                Code = codigo,
                DocumentId = documentId,
            };

            var response = await _httpClient
              .PostAsJsonAsync(url, request, cancellationToken)
              .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                throw await MapHttpError(response);

            return await response.Content.ReadAsByteArrayAsync(cancellationToken);
        }
        catch (TaskCanceledException ex)
        {
            throw new InfrastructureException(
                FirmaErrors.FIRD_Timeout,
                (int)HttpStatusCode.GatewayTimeout,
                ex);
        }
        catch (HttpRequestException ex)
        {
            throw new InfrastructureException(
                FirmaErrors.FIRD_ConnectionError,
                (int)HttpStatusCode.BadGateway,
                ex);
        }
    }


    #region METODOS PRIVADOS
    private static async Task<InfrastructureException> MapHttpError(
        HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        return response.StatusCode switch
        {
            HttpStatusCode.Unauthorized =>
                new InfrastructureException(
                    FirmaErrors.FIRD_Unauthorized,
                    (int)HttpStatusCode.Unauthorized),

            HttpStatusCode.Forbidden =>
                new InfrastructureException(
                    FirmaErrors.FIRD_Forbidden,
                    (int)HttpStatusCode.Forbidden),

            HttpStatusCode.NotFound =>
                new InfrastructureException(
                    FirmaErrors.FIRD_NotFound,
                    (int)HttpStatusCode.NotFound),

            HttpStatusCode.InternalServerError =>
                new InfrastructureException(
                    FirmaErrors.FIRD_InternalServerError,
                    (int)HttpStatusCode.BadGateway),

            _ =>
                new InfrastructureException(
                    FirmaErrors.FIRD_HttpErrorWithDetail(content),
                    (int)response.StatusCode)
        };
    }

    private static void AddIfHasValue(MultipartFormDataContent formData, string key, object? value)
    {
        if (value is null)
            return;

        var stringValue = value switch
        {
            bool b => b.ToString().ToLowerInvariant(),
            _ => value.ToString()
        };

        if (!string.IsNullOrWhiteSpace(stringValue))
            formData.Add(new StringContent(stringValue, Encoding.UTF8), key);
    }


    private MultipartFormDataContent BuildRequestAsync(FirmarDocumentoRequestDto request, bool esPdf, bool esZip)
    {
        var formData = new MultipartFormDataContent
        {
            { new StringContent(_options.CodigoSistema, Encoding.UTF8), "CodigoSistema" },
            { new StringContent(_options.EndpointCallBack + "api/v1/solicitudes/firma/interna/resultado", Encoding.UTF8), "CallBackUrl" },
            { new StringContent(request.CodigoArchivo, Encoding.UTF8), "CodigoArchivo" }
        };

        if (!string.IsNullOrWhiteSpace(request.Referencia))
            formData.Add(new StringContent(request.Referencia, Encoding.UTF8), "Referencia");


        AddIfHasValue(formData, "TipoFirma", request.TipoFirma);
        AddIfHasValue(formData, "ModoFirma", request.ModoFirma);
        AddIfHasValue(formData, "Tsp", request.Tsp);
        AddIfHasValue(formData, "Tags", request.Tags);
        AddIfHasValue(formData, "CoordenadaX", request.VisSigX);
        AddIfHasValue(formData, "CoordenadaY", request.VisSigY);
        AddIfHasValue(formData, "Ancho", request.VisSigWidth);
        AddIfHasValue(formData, "Alto", request.VisSigHeight);
        AddIfHasValue(formData, "UrlImagen", request.VisSigGraphic);
        AddIfHasValue(formData, "Texto", request.VisSigText);
        AddIfHasValue(formData, "TamanioTexto", request.VisSigTextSize);
        AddIfHasValue(formData, "UbicacionPaginaFirma", request.VisSigPage);
        AddIfHasValue(formData, "Ltv", request.Ltv);
        AddIfHasValue(formData, "Background", request.Bg);
        AddIfHasValue(formData, "Rotacion", request.VisSigRotation);

        var streamContent = new StreamContent(request.ArchivoStream);

        if (esPdf)
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        if (esZip)
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/zip");

        formData.Add(streamContent, "File", request.CodigoArchivo);

        return formData;
    }

    #endregion
}
