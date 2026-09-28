using COFIDE.WFF.Application.Common.Errors;
namespace COFIDE.WFF.Infrastructure.Adapters.ExternalServices.Correo;

public static class CorreoErrors
{
    public static readonly ApplicationError Unauthorized =
        new("API_CORREO_401", "Token inválido o expirado en Api Correo.");

    public static readonly ApplicationError Forbidden =
        new("API_CORREO_403", "El usuario no tiene permisos en Api Correo.");

    public static readonly ApplicationError NotFound =
        new("API_CORREO_404", "Endpoint Api Correo no encontrado.");

    public static readonly ApplicationError InternalServerError =
        new("API_CORREO_500", "Error interno en Api Correo.");

    public static readonly ApplicationError Timeout =
        new("API_CORREO_TIMEOUT", "Timeout al consumir Api Correo.");

    public static readonly ApplicationError ConnectionError =
        new("API_CORREO_CONNECTION_ERROR", "Error de conexión con Api Correo.");

    public static readonly ApplicationError EmptyResponse =
        new("API_CORREO_EMPTY", "Api Correo respondió vacío.");

    public static readonly ApplicationError HttpError =
        new("API_CORREO_HTTP_ERROR", "Api Correo respondió error HTTP.");

    public static ApplicationError LogicalError(string? code, string? message) =>
      new(
          $"API_CORREO_LOGIC_{code ?? "UNKNOWN"}",
          message ?? "Api Correo respondió error lógico.");

    public static ApplicationError HttpErrorWithDetail(string detail) =>
        new(
            "API_CORREO_HTTP_ERROR",
            $"Api Correo respondió error HTTP. Detalle: {detail}");
}
