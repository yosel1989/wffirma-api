using COFIDE.WFF.Application.Ports.Out;
using COFIDE.WFF.Infrastructure.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
namespace COFIDE.WFF.Infrastructure.Adapters.Services.FileStorage;

public class FileStorageService(
        IOptions<FileStorageOptions> options,
        ILogger<FileStorageOptions> logger
    ) : IFileStorageService
{
    private readonly FileStorageOptions _options = options.Value;
    private readonly ILogger<FileStorageOptions> _logger = logger;

    public async Task<string> SaveFileAsync(
            Stream fileStream,
            string originalFileName,
            string subPath,
            string newFileName,
            CancellationToken cancellationToken = default)
    {

        string basePath = _options.BaseUrl;

        try
        {
            if (fileStream == null)
                throw new InfrastructureException(FileStorageErrors.FileEmpty);

            if (string.IsNullOrEmpty(basePath))
                throw new InfrastructureException(FileStorageErrors.BasePathEmpty);

            if (string.IsNullOrEmpty(newFileName))
                throw new InfrastructureException(FileStorageErrors.FileNameEmpty);

            // Construir la ruta completa
            var fullPath = Path.Combine(basePath, subPath);

            // Crear directorio si no existe
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }

            // Construir ruta completa del archivo
            var filePath = Path.Combine(fullPath, newFileName);

            // Guardar archivo
            using (var fileStreamWriter = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                await fileStream.CopyToAsync(fileStreamWriter, cancellationToken);
            }
            return filePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar archivo: {OriginalFileName} en {BasePath}/{SubPath}",
                originalFileName, basePath, subPath);

            throw new InfrastructureException(FileStorageErrors.Unexpected, inner: ex);
        }
    }

    public Task<Stream> GetFileAsync(string filePath, CancellationToken cancellationToken = default)
    {

        string basePath = _options.BaseUrl;
        var fullPath = Path.Combine(basePath, filePath);

        try
        {
            if (string.IsNullOrEmpty(fullPath))
                throw new InfrastructureException(FileStorageErrors.FullPathEmpty);

            if (!File.Exists(fullPath))
                throw new InfrastructureException(FileStorageErrors.NotFoundFile);

            _logger.LogInformation("Obteniendo archivo: {FilePath}", fullPath);

            Stream stream = new FileStream(
              fullPath,
              FileMode.Open,
              FileAccess.Read,
              FileShare.Read,
              bufferSize: 81920,
              useAsync: true);

            return Task.FromResult(stream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener archivo: {FilePath}", fullPath);
            throw new InfrastructureException(FileStorageErrors.UnexpectedGetFile, inner: ex);
        }
    }

    public Task<bool> DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {

        string basePath = _options.BaseUrl;
        var fullPath = Path.Combine(basePath, filePath);

        try
        {
            if (string.IsNullOrEmpty(fullPath))
                throw new InfrastructureException(FileStorageErrors.FullPathEmpty);

            if (!File.Exists(fullPath))
            {
                _logger.LogWarning("Archivo no existe para eliminar: {FilePath}", fullPath);
                return Task.FromResult(false);
            }

            File.Delete(fullPath);
            _logger.LogInformation("Archivo eliminado exitosamente: {FilePath}", fullPath);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar archivo: {FilePath}", fullPath);
            throw new InfrastructureException(FileStorageErrors.UnexpectedDeleteFile, inner: ex);
        }
    }

    public Task<bool> ExistsFileAsync(
    string ruta,
    string nombre,
    string extension,
    CancellationToken cancellationToken = default)
    {
        string basePath = _options.BaseUrl;

        if (string.IsNullOrWhiteSpace(ruta))
            return Task.FromResult(false);

        if (string.IsNullOrWhiteSpace(nombre))
            return Task.FromResult(false);

        if (string.IsNullOrWhiteSpace(extension))
            return Task.FromResult(false);

        var fileName = $"{nombre}.{extension}";
        var fullPath = Path.Combine(basePath, ruta, fileName);

        return Task.FromResult(File.Exists(fullPath));
    }

    public Task<string> GetRouteAsync(string filePath, CancellationToken cancellationToken = default)
    {

        string basePath = _options.BaseUrl;
        var fullPath = Path.Combine(basePath, filePath);

        return Task.FromResult(fullPath);
    }

    // Estructura de carpetas del almacenamiento: es conocimiento del adaptador, no de Application.
    private const string CarpetaDocumentos = "DOCUMENTOS";
    private const string CarpetaSustentos = "SUSTENTOS";

    public string ObtenerCarpetaDocumentos(string codigoSolicitud) =>
        Path.Combine(codigoSolicitud, CarpetaDocumentos);

    public string ObtenerCarpetaSustentos(string codigoSolicitud) =>
        Path.Combine(codigoSolicitud, CarpetaSustentos);

    public string CombinarRuta(string carpeta, string nombreArchivo) =>
        Path.Combine(carpeta, nombreArchivo);
}
