namespace COFIDE.WFF.Application.Ports.Out;

/// <summary>
/// Servicio para el manejo de archivos y almacenamiento
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Guarda un archivo en la ruta especificada
    /// </summary>
    /// <param name="fileStream">Stream del archivo a guardar</param>
    /// <param name="originalFileName">Nombre original del archivo</param>
    /// <param name="basePath">Ruta base donde guardar</param>
    /// <param name="subPath">Subdirectorio (ej: "idIfi/desembolso/entrada")</param>
    /// <param name="newFileName">Nuevo nombre del archivo</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Ruta completa del archivo guardado</returns>
    Task<string> SaveFileAsync(
        Stream fileStream,
        string originalFileName,
        string subPath,
        string newFileName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un archivo desde la ruta especificada
    /// </summary>
    /// <param name="filePath">Ruta completa del archivo</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Stream del archivo</returns>
    Task<Stream> GetFileAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina un archivo de la ruta especificada
    /// </summary>
    /// <param name="filePath">Ruta completa del archivo</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si se eliminó correctamente</returns>
    Task<bool> DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);
    Task<bool> ExistsFileAsync(
    string ruta,
    string nombre,
    string extension,
    CancellationToken cancellationToken = default);
    Task<string> GetRouteAsync(string filePath, CancellationToken cancellationToken = default);

    string ObtenerCarpetaDocumentos(string codigoSolicitud);
    string ObtenerCarpetaSustentos(string codigoSolicitud);
    string CombinarRuta(string carpeta, string nombreArchivo);
}

