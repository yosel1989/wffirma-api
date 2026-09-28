using COFIDE.WFF.Application.Common.Errors;
namespace COFIDE.WFF.Infrastructure.Adapters.ExternalServices.Firmas;

public static class FirmaErrors
{
    public static readonly ApplicationError FIRD_Unauthorized =
        new("FIRD_401", "Token inválido o expirado en FIRD.");

    public static readonly ApplicationError FIRD_Forbidden =
        new("FIRD_403", "El usuario no tiene permisos en FIRD.");

    public static readonly ApplicationError FIRD_NotFound =
        new("FIRD_404", "Endpoint FIRD no encontrado.");

    public static readonly ApplicationError FIRD_InternalServerError =
        new("FIRD_500", "Error interno en el servicio FIRD.");

    public static readonly ApplicationError FIRD_Timeout =
        new("FIRD_TIMEOUT", "Timeout al consumir el servicio FIRD.");

    public static readonly ApplicationError FIRD_ConnectionError =
        new("FIRD_CONNECTION_ERROR", "Error de conexión al servicio FIRD.");

    public static readonly ApplicationError FIRD_EmptyResponse =
        new("FIRD_EMPTY_RESPONSE", "FIRD respondió vacío o sin datos válidos.");

    public static readonly ApplicationError FIRD_Unexpected =
        new("FIRD_UNEXPECTED_ERROR", "Error inesperado al consumir FIRD.");

    public static readonly ApplicationError FIRD_HttpError =
        new("FIRD_HTTP_ERROR", "FIRD respondió error HTTP.");

    public static readonly ApplicationError FIRD_AuthFailed =
        new("FIRD_AUTH_FAILED", "No se pudo autenticar en FIRD.");

    public static readonly ApplicationError FIRD_UserDataMissing =
        new("FIRD_USER_DATA_MISSING", "No se obtuvo información válida del usuario en FIRD.");

    public static ApplicationError FIRD_LogicalError(string? code, string? message) =>
        new(
            $"FIRD_LOGIC_{code ?? "UNKNOWN"}",
            message ?? "FIRD respondió error lógico.");

    public static ApplicationError FIRD_HttpErrorWithDetail(string detail) =>
        new(
            "FIRD_HTTP_ERROR",
            $"FIRD respondió error HTTP. Detalle: {detail}");
}
