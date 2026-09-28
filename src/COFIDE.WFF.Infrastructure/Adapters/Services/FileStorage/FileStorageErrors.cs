using COFIDE.WFF.Application.Common.Errors;
namespace COFIDE.WFF.Infrastructure.Adapters.Services.FileStorage;


public static class FileStorageErrors
{
    public static readonly ApplicationError Unexpected =
        new("FILESERVER_UNEXPECTED_ERROR", "Error inesperado al consumir FileStorage.");

    public static readonly ApplicationError NotFoundFile =
         new("FILESERVER_NOTFOUND_FILE_ERROR", "No se encontró el archivo");

    public static readonly ApplicationError UnexpectedGetFile =
         new("FILESERVER_UNEXPECTED_GETFILE_ERROR", "Error al obtener el archivo FileStorage.");

    public static readonly ApplicationError UnexpectedDeleteFile =
         new("FILESERVER_UNEXPECTED_DELETEFILE_ERROR", "Error al eliminar el archivo FileStorage.");



    public static readonly ApplicationError HttpError =
        new("SUC_HTTP_ERROR", "SUC respondió error HTTP.");

    public static readonly ApplicationError AuthFailed =
        new("SUC_AUTH_FAILED", "No se pudo autenticar en SUC.");

    public static readonly ApplicationError UserDataMissing =
        new("SUC_USER_DATA_MISSING", "No se obtuvo información válida del usuario en SUC.");

    public static ApplicationError LogicalError(string? code, string? message) =>
        new(
            $"SUC_LOGIC_{code ?? "UNKNOWN"}",
            message ?? "SUC respondió error lógico.");

    public static ApplicationError HttpErrorWithDetail(string detail) =>
        new(
            "SUC_HTTP_ERROR",
            $"SUC respondió error HTTP. Detalle: {detail}");


        
    public static readonly ApplicationError BasePathEmpty = new("DOM_FS_001", "Base path no puede estar vacío.");
    public static readonly ApplicationError FileEmpty = new("DOM_FS_002", "Archivo no válido.");
    public static readonly ApplicationError FileNameEmpty = new("DOM_FS_003", "Nombre de archivo se encuentra vacío.");
    public static readonly ApplicationError FullPathEmpty = new("DOM_FS_004", "Ruta de archivo se encuentra vacío");
}