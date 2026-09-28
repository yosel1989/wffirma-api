using COFIDE.WFF.Infrastructure.Adapters.ExternalServices.Suc.DTOs;
using COFIDE.WFF.Infrastructure.Exceptions;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Json;
namespace COFIDE.WFF.Infrastructure.Adapters.ExternalServices.Suc;

public class SucApiClient(
        HttpClient httpClient,
        IOptions<SucApiOptions> options
    )
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly SucApiOptions _options = options.Value;

    public async Task<IngresarSistemaResponseDto?> IngresarSistemaAsync(string codigoUsuario, CancellationToken cancellationToken = default)
    {
        try
        {

            var url =
                $"api/v2/cuentas-usuarios/ingresar-sistema" +
                $"?CodigoUsuario={codigoUsuario}&CodigoSistema={_options.CodigoSistema}";

            var response = await _httpClient
              .GetAsync(url, cancellationToken)
              .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                throw await MapHttpError(response);

            var wrapper = await response.Content
              .ReadFromJsonAsync<SucResponseDto<IngresarSistemaResponseDto>>(cancellationToken: cancellationToken)
              ?? throw new InfrastructureException(SucErrors.EmptyResponse);

            if (!wrapper.Success)
                throw new InfrastructureException(
                    SucErrors.LogicalError(wrapper.Code, wrapper.Message),
                    (int)response.StatusCode);

            if (wrapper.Data is null || wrapper.Data.JwtToken is null)
                throw new InfrastructureException(SucErrors.EmptyResponse);

            return wrapper.Data;
        }
        catch (TaskCanceledException ex)
        {
            throw new InfrastructureException(
                SucErrors.Timeout,
                (int)HttpStatusCode.GatewayTimeout,
                ex);
        }
        catch (HttpRequestException ex)
        {
            throw new InfrastructureException(
                SucErrors.ConnectionError,
                (int)HttpStatusCode.BadGateway,
                ex);
        }
    }

    public async Task<ObtenerDatosUsuarioPorCodigoResponseDto?> ObtenerDatosUsuarioPorCodigo(string codigoUsuario, CancellationToken cancellationToken = default)
    {
        try
        {
            var url =
                $"api/v1/usuarios/obtener-por-codigo" +
                $"?CodigoSistema={_options.CodigoSistema}&CodigoUsuario={codigoUsuario}";

            var response = await _httpClient
               .GetAsync(url, cancellationToken)
               .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                throw await MapHttpError(response);

            var wrapper = await response.Content
                .ReadFromJsonAsync<SucResponseDto<ObtenerDatosUsuarioPorCodigoResponseDto>>(cancellationToken: cancellationToken)
                ?? throw new InfrastructureException(SucErrors.EmptyResponse);

            if (!wrapper.Success)
                throw new InfrastructureException(
                    SucErrors.LogicalError(wrapper.Code, wrapper.Message),
                    (int)response.StatusCode);

            if (wrapper.Data is null)
                throw new InfrastructureException(SucErrors.EmptyResponse);

            return wrapper.Data;
        }
        catch (TaskCanceledException ex)
        {
            throw new InfrastructureException(
                SucErrors.Timeout,
                (int)HttpStatusCode.GatewayTimeout,
                ex);
        }
        catch (HttpRequestException ex)
        {
            throw new InfrastructureException(
                SucErrors.ConnectionError,
                (int)HttpStatusCode.BadGateway,
                ex);
        }
    }

    public async Task<bool> CerrarSesionAsync(CancellationToken cancellationToken = default)
    {
        try
        {

            var url = $"api/v2/cuentas-usuarios/cerrar-sesion";

            var response = await _httpClient
              .PostAsync(url, null, cancellationToken)
              .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                throw await MapHttpError(response);

            var wrapper = await response.Content.ReadFromJsonAsync<SucResponseDto<bool>>(cancellationToken: cancellationToken);

            if (wrapper is null)
                throw new InfrastructureException(SucErrors.EmptyResponse);

            if (!wrapper.Success)
                throw new InfrastructureException(
                    SucErrors.LogicalError(wrapper.Code, wrapper.Message),
                    (int)response.StatusCode);

            return wrapper.Data;

        }
        catch (TaskCanceledException ex)
        {
            throw new InfrastructureException(
                SucErrors.Timeout,
                (int)HttpStatusCode.GatewayTimeout,
                ex);
        }
        catch (HttpRequestException ex)
        {
            throw new InfrastructureException(
                SucErrors.ConnectionError,
                (int)HttpStatusCode.BadGateway,
                ex);
        }
    }

    public async Task<List<ObtenerParametroDetalleResponseDto>>
    ObtenerParametroDetalleAsync(
        string codigoConfiguracion,
        CancellationToken cancellationToken = default)
    {
        try
        {

            var url =
                $"api/v2/configuracion/listar-configuraciones" +
                $"?CodigoSistema={_options.CodigoSistema}" +
                $"&CodigoConfiguracion={codigoConfiguracion}";


            var response =
                await _httpClient.GetAsync(
                    url,
                    cancellationToken);


            if (!response.IsSuccessStatusCode)
                throw await MapHttpError(response);


            var wrapper =
                await response.Content
                .ReadFromJsonAsync<
                    SucResponseDto<
                        List<ObtenerParametroDetalleResponseDto>>>(
                            cancellationToken: cancellationToken)
                ?? throw new InfrastructureException(
                    SucErrors.EmptyResponse);



            if (!wrapper.Success)
                throw new InfrastructureException(
                    SucErrors.LogicalError(
                        wrapper.Code,
                        wrapper.Message),
                    (int)response.StatusCode);



            return wrapper.Data ??
                throw new InfrastructureException(
                    SucErrors.EmptyResponse);

        }
        catch (TaskCanceledException ex)
        {
            throw new InfrastructureException(
                SucErrors.Timeout,
                (int)HttpStatusCode.GatewayTimeout,
                ex);
        }
        catch (HttpRequestException ex)
        {
            throw new InfrastructureException(
                SucErrors.ConnectionError,
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
                    SucErrors.Unauthorized,
                    (int)HttpStatusCode.Unauthorized),

            HttpStatusCode.Forbidden =>
                new InfrastructureException(
                    SucErrors.Forbidden,
                    (int)HttpStatusCode.Forbidden),

            HttpStatusCode.NotFound =>
                new InfrastructureException(
                    SucErrors.NotFound,
                    (int)HttpStatusCode.NotFound),

            HttpStatusCode.InternalServerError =>
                new InfrastructureException(
                    SucErrors.InternalServerError,
                    (int)HttpStatusCode.BadGateway),

            _ =>
                new InfrastructureException(
                    SucErrors.HttpErrorWithDetail(content),
                    (int)response.StatusCode)
        };
    }
}
