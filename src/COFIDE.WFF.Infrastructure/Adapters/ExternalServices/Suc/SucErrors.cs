using COFIDE.WFF.Application.Common.Errors;
namespace COFIDE.WFF.Infrastructure.Adapters.ExternalServices.Suc;

public static class SucErrors
{
    public static readonly ApplicationError Unauthorized =
        new("SUC_401", "Token inválido o expirado en SUC.");

    public static readonly ApplicationError Forbidden =
        new("SUC_403", "El usuario no tiene permisos en SUC.");

    public static readonly ApplicationError NotFound =
        new("SUC_404", "Endpoint SUC no encontrado.");

    public static readonly ApplicationError InternalServerError =
        new("SUC_500", "Error interno en el servicio SUC.");

    public static readonly ApplicationError Timeout =
        new("SUC_TIMEOUT", "Timeout al consumir el servicio SUC.");

    public static readonly ApplicationError ConnectionError =
        new("SUC_CONNECTION_ERROR", "Error de conexión al servicio SUC.");

    public static readonly ApplicationError EmptyResponse =
        new("SUC_EMPTY_RESPONSE", "SUC respondió vacío o sin datos válidos.");

    public static readonly ApplicationError Unexpected =
        new("SUC_UNEXPECTED_ERROR", "Error inesperado al consumir SUC.");

    public static readonly ApplicationError HttpError =
        new("SUC_HTTP_ERROR", "SUC respondió error HTTP.");

    public static readonly ApplicationError AuthFailed =
        new("SUC_AUTH_FAILED", "No se pudo autenticar en SUC.");

    public static readonly ApplicationError UserDataMissing =
        new("SUC_USER_DATA_MISSING", "No se obtuvo información válida del usuario en SUC.");

    public static readonly ApplicationError SucWcfErrorsNull =
        new("SUC_WCF_EMPTY_RESPONSE", "El servicio SUC devolvió NULL.");

    public static ApplicationError LogicalError(string? code, string? message) =>
        new(
            $"SUC_LOGIC_{code ?? "UNKNOWN"}",
            message ?? "SUC respondió error lógico.");

    public static ApplicationError HttpErrorWithDetail(string detail) =>
        new(
            "SUC_HTTP_ERROR",
            $"SUC respondió error HTTP. Detalle: {detail}");
}