namespace COFIDE.WFF.Application.Features.Solicitudes.Services;

    public interface ISolicitudDescargaZipService
    {
    Task<(byte[] Archivo, string NombreZip)> DescargarDocumentosZipAsync(
        long idSolicitud,
        List<long>? idsDocumentos,
        CancellationToken cancellationToken);

    Task<(byte[] Archivo, string NombreZip)> DescargarSustentosZipAsync(
        long idSolicitud,
        List<long>? idsSustentos,
        CancellationToken cancellationToken);

    Task<(byte[] Archivo, string NombreZip)> DescargarTodoZipAsync(
        long idSolicitud,
        CancellationToken cancellationToken);

}

