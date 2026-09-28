using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using System.IO.Compression;
using static System.Runtime.InteropServices.JavaScript.JSType;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;

namespace COFIDE.WFF.Application.Features.Solicitudes.Services;

public sealed class SolicitudDescargaZipService(
        ISolicitudRepository solicitudRepository,
        IFileStorageService fileStorageService
    ) : ISolicitudDescargaZipService
{
    private readonly ISolicitudRepository _solicitudRepository = solicitudRepository;
    private readonly IFileStorageService _fileStorageService = fileStorageService;

    public async Task<(byte[] Archivo, string NombreZip)> DescargarDocumentosZipAsync(
        long idSolicitud,
        List<long>? idsDocumentos,
        CancellationToken cancellationToken)
    {
        var solicitud = await _solicitudRepository.ObtenerSolicitudConDocumentosYSustentosPorIdAsync(
            idSolicitud,
             idsDocumentos,
                null,
            cancellationToken,
            false);

        if (solicitud is null)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.SolicitudInvalid);

        var documentosActivos = solicitud.Documentos
            .Where(x => x.Estado)
            .ToList();

        if (documentosActivos.Count == 0)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.ArchivoNotFound);

        using var memoryStream = new MemoryStream();

        using (var zip = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            int contador = 1;
            var nombresUsados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var documento in documentosActivos)
            {
                var archivoInfo = ResolverArchivoDocumento(documento, solicitud.InCodigo);

                await using var fileStream = await _fileStorageService.GetFileAsync(
                    archivoInfo.RutaRelativa,
                    cancellationToken);

                var nombreEntry = GenerarNombreUnicoEntry(
                    nombresUsados,
                    contador++,
                    archivoInfo.NombreCompleto);

                var entry = zip.CreateEntry($"DOCUMENTOS/{nombreEntry}", CompressionLevel.Fastest);

                await using var entryStream = entry.Open();
                await fileStream.CopyToAsync(entryStream, cancellationToken);
            }
        }

        return (memoryStream.ToArray(), $"DOCUMENTOS_{solicitud.CodigoSolicitud}.zip");
    }

    public async Task<(byte[] Archivo, string NombreZip)> DescargarSustentosZipAsync(
        long idSolicitud,
        List<long>? idsSustentos,
        CancellationToken cancellationToken)
    {
        var solicitud = await _solicitudRepository.ObtenerSolicitudConDocumentosYSustentosPorIdAsync(
            idSolicitud,
            null,
            idsSustentos,
            cancellationToken,
            false);

        if (solicitud is null)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.SolicitudInvalid);

        var sustentosActivos = solicitud.Sustentos
            .Where(x => x.Estado)
            .ToList();

        if (sustentosActivos.Count == 0)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.ArchivoNotFound);

        using var memoryStream = new MemoryStream();

        using (var zip = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            int contador = 1;
            var nombresUsados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var sustento in sustentosActivos)
            {
                string nombreArchivo = $"{sustento.NombreSustento}.{sustento.ExtensionSustento}";
                string rutaRelativa = Path.Combine(sustento.RutaSustento, nombreArchivo);

                await using var fileStream = await _fileStorageService.GetFileAsync(
                    rutaRelativa,
                    cancellationToken);

                var nombreEntry = GenerarNombreUnicoEntry(
                    nombresUsados,
                    contador++,
                    nombreArchivo);

                var entry = zip.CreateEntry($"SUSTENTOS/{nombreEntry}", CompressionLevel.Fastest);

                await using var entryStream = entry.Open();
                await fileStream.CopyToAsync(entryStream, cancellationToken);
            }
        }

        return (memoryStream.ToArray(), $"SUSTENTOS_{solicitud.CodigoSolicitud}.zip");
    }

    private static (string RutaRelativa, string NombreCompleto) ResolverArchivoDocumento(
        SolicitudDocumento documento,
        bool inCodigo)
    {
        var detalleFirmado = documento.DetallesFirma?
            .Where(x =>
                x.Estado &&
                !string.IsNullOrWhiteSpace(x.RutaDocumentoFirmado) &&
                !string.IsNullOrWhiteSpace(x.NombreDocumentoFirmado) &&
                !string.IsNullOrWhiteSpace(x.ExtensionDocumentoFirmado))
            .OrderByDescending(x => x.FechaHoraFirma)
            .FirstOrDefault();

        if (detalleFirmado is not null)
        {
            string nombreFirmado = $"{detalleFirmado.NombreDocumentoFirmado}.{detalleFirmado.ExtensionDocumentoFirmado}";
            string rutaFirmado = Path.Combine(detalleFirmado.RutaDocumentoFirmado!, nombreFirmado);

            return (rutaFirmado, nombreFirmado);
        }
        else if (!string.IsNullOrWhiteSpace(documento.RutaConTag) &&
            !string.IsNullOrWhiteSpace(documento.NombreConTag) &&
            !string.IsNullOrWhiteSpace(documento.ExtensionConTag))
        {

            string nombreFirmado = $"{documento.NombreConTag}.{documento.ExtensionConTag}";
            string rutaFirmado = Path.Combine(documento.RutaConTag!, nombreFirmado);

            return (rutaFirmado, nombreFirmado);
        }
        if (inCodigo &&
            !string.IsNullOrWhiteSpace(documento.RutaCodigo) &&
            !string.IsNullOrWhiteSpace(documento.NombreCodigo) &&
            !string.IsNullOrWhiteSpace(documento.ExtensionCodigo))
        {
            string nombreCodigo = $"{documento.NombreCodigo}.{documento.ExtensionCodigo}";
            string rutaCodigo = Path.Combine(documento.RutaCodigo, nombreCodigo);

            return (rutaCodigo, nombreCodigo);
        }

        string nombreOriginal = $"{documento.NombreOriginal}.{documento.ExtensionOriginal}";
        string rutaOriginal = Path.Combine(documento.RutaOriginal, nombreOriginal);

        return (rutaOriginal, nombreOriginal);
    }

    private static string GenerarNombreUnicoEntry(
        HashSet<string> nombresUsados,
        int correlativo,
        string nombreOriginal)
    {
        if (!nombresUsados.Contains(nombreOriginal))
        {
            nombresUsados.Add(nombreOriginal);
            return nombreOriginal;
        }

        string nombreSinExtension = Path.GetFileNameWithoutExtension(nombreOriginal);
        string extension = Path.GetExtension(nombreOriginal);

        string nuevoNombre;

        do
        {
            nuevoNombre = $"{nombreSinExtension}_{correlativo}{extension}";
            correlativo++;
        }
        while (nombresUsados.Contains(nuevoNombre));

        nombresUsados.Add(nuevoNombre);

        return nuevoNombre;
    }

    public async Task<(byte[] Archivo, string NombreZip)> DescargarTodoZipAsync(long idSolicitud, CancellationToken cancellationToken)
    {
        var solicitud = await _solicitudRepository.ObtenerSolicitudConDocumentosYSustentosPorIdAsync(
            idSolicitud,
            null,
            null,
            cancellationToken,
            false);

        if (solicitud is null)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.SolicitudInvalid);

        var documentosActivos = solicitud.Documentos
            .Where(x => x.Estado)
            .ToList();

        var sustentosActivos = solicitud.Sustentos
            .Where(x => x.Estado)
            .ToList();

        if (documentosActivos.Count == 0 && sustentosActivos.Count == 0)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.ArchivoNotFound);

        using var memoryStream = new MemoryStream();

        using (var zip = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            int contadorDocumentos = 1;
            int contadorSustentos = 1;

            var nombresDocs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var nombresSust = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var documento in documentosActivos)
            {
                var archivoInfo = ResolverArchivoDocumento(documento, solicitud.InCodigo);

                await using var fileStream = await _fileStorageService.GetFileAsync(
                    archivoInfo.RutaRelativa,
                    cancellationToken);

                var nombreEntry = GenerarNombreUnicoEntry(
                    nombresDocs,
                    contadorDocumentos++,
                    archivoInfo.NombreCompleto);

                var entry = zip.CreateEntry(
                    $"DOCUMENTOS/{nombreEntry}",
                    CompressionLevel.Fastest);

                await using var entryStream = entry.Open();
                await fileStream.CopyToAsync(entryStream, cancellationToken);
            }

            foreach (var sustento in sustentosActivos)
            {
                string nombreArchivo = $"{sustento.NombreSustento}.{sustento.ExtensionSustento}";
                string rutaRelativa = Path.Combine(
                    sustento.RutaSustento,
                    nombreArchivo);

                await using var fileStream = await _fileStorageService.GetFileAsync(
                    rutaRelativa,
                    cancellationToken);

                var nombreEntry = GenerarNombreUnicoEntry(
                    nombresSust,
                    contadorSustentos++,
                    nombreArchivo);

                var entry = zip.CreateEntry(
                    $"SUSTENTOS/{nombreEntry}",
                    CompressionLevel.Fastest);

                await using var entryStream = entry.Open();
                await fileStream.CopyToAsync(entryStream, cancellationToken);
            }
        }

        return (
            memoryStream.ToArray(),
            $"{solicitud.CodigoSolicitud}.zip"
        );
    }
}