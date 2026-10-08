using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;

using COFIDE.WFF.Application.Common.Files;
namespace COFIDE.WFF.Application.Features.Solicitudes.Services;

/// <summary>
/// Decide QUÉ archivos de una solicitud van en el ZIP y con qué nombre.
/// CÓMO se arma el ZIP y cómo se leen los archivos es responsabilidad de Infraestructura
/// (IZipStreamingService / IFileStorageService).
/// </summary>
public sealed class SolicitudDescargaZipService(
        ISolicitudRepository solicitudRepository,
        IFileStorageService fileStorageService,
        IZipStreamingService zipStreamingService
    ) : ISolicitudDescargaZipService
{
    private const string CarpetaZipDocumentos = "DOCUMENTOS";
    private const string CarpetaZipSustentos = "SUSTENTOS";

    private readonly ISolicitudRepository _solicitudRepository = solicitudRepository;
    private readonly IFileStorageService _fileStorageService = fileStorageService;
    private readonly IZipStreamingService _zipStreamingService = zipStreamingService;

    public async Task<(byte[] Archivo, string NombreZip)> DescargarDocumentosZipAsync(
        long idSolicitud,
        List<long>? idsDocumentos,
        CancellationToken cancellationToken)
    {
        var solicitud = await ObtenerSolicitudAsync(idSolicitud, idsDocumentos, null, cancellationToken);

        var documentosActivos = solicitud.Documentos.Where(x => x.Estado).ToList();

        if (documentosActivos.Count == 0)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.ArchivoNotFound);

        var entradas = EntradasDocumentos(documentosActivos, solicitud.InCodigo);
        var archivo = await _zipStreamingService.GenerarZipAsync(entradas, cancellationToken);

        return (archivo, $"DOCUMENTOS_{solicitud.CodigoSolicitud}.zip");
    }

    public async Task<(byte[] Archivo, string NombreZip)> DescargarSustentosZipAsync(
        long idSolicitud,
        List<long>? idsSustentos,
        CancellationToken cancellationToken)
    {
        var solicitud = await ObtenerSolicitudAsync(idSolicitud, null, idsSustentos, cancellationToken);

        var sustentosActivos = solicitud.Sustentos.Where(x => x.Estado).ToList();

        if (sustentosActivos.Count == 0)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.ArchivoNotFound);

        var entradas = EntradasSustentos(sustentosActivos);
        var archivo = await _zipStreamingService.GenerarZipAsync(entradas, cancellationToken);

        return (archivo, $"SUSTENTOS_{solicitud.CodigoSolicitud}.zip");
    }

    public async Task<(byte[] Archivo, string NombreZip)> DescargarTodoZipAsync(long idSolicitud, CancellationToken cancellationToken)
    {
        var solicitud = await ObtenerSolicitudAsync(idSolicitud, null, null, cancellationToken);

        var documentosActivos = solicitud.Documentos.Where(x => x.Estado).ToList();
        var sustentosActivos = solicitud.Sustentos.Where(x => x.Estado).ToList();

        if (documentosActivos.Count == 0 && sustentosActivos.Count == 0)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.ArchivoNotFound);

        var entradas = EntradasDocumentos(documentosActivos, solicitud.InCodigo)
            .Concat(EntradasSustentos(sustentosActivos))
            .ToList();

        var archivo = await _zipStreamingService.GenerarZipAsync(entradas, cancellationToken);

        return (archivo, $"{solicitud.CodigoSolicitud}.zip");
    }

    private async Task<Solicitud> ObtenerSolicitudAsync(
        long idSolicitud,
        List<long>? idsDocumentos,
        List<long>? idsSustentos,
        CancellationToken cancellationToken)
    {
        var solicitud = await _solicitudRepository.ObtenerSolicitudConDocumentosYSustentosPorIdAsync(
            idSolicitud,
            idsDocumentos,
            idsSustentos,
            cancellationToken,
            false);

        return solicitud ?? throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.SolicitudInvalid);
    }

    private List<EntradaZip> EntradasDocumentos(IEnumerable<SolicitudDocumento> documentos, bool inCodigo)
    {
        var entradas = new List<EntradaZip>();
        var nombresUsados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var contador = 1;

        foreach (var documento in documentos)
        {
            var (rutaArchivo, nombreArchivo) = ResolverArchivoDocumento(documento, inCodigo);
            var nombreEntry = GenerarNombreUnicoEntry(nombresUsados, contador++, nombreArchivo);
            entradas.Add(new EntradaZip($"{CarpetaZipDocumentos}/{nombreEntry}", rutaArchivo));
        }

        return entradas;
    }

    private List<EntradaZip> EntradasSustentos(IEnumerable<SolicitudSustento> sustentos)
    {
        var entradas = new List<EntradaZip>();
        var nombresUsados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var contador = 1;

        foreach (var sustento in sustentos)
        {
            string nombreArchivo = $"{sustento.NombreSustento}.{sustento.ExtensionSustento}";
            string rutaArchivo = _fileStorageService.CombinarRuta(sustento.RutaSustento, nombreArchivo);
            var nombreEntry = GenerarNombreUnicoEntry(nombresUsados, contador++, nombreArchivo);
            entradas.Add(new EntradaZip($"{CarpetaZipSustentos}/{nombreEntry}", rutaArchivo));
        }

        return entradas;
    }

    /// <summary>
    /// Versión del documento a entregar: firmada &gt; con tag &gt; con código (si aplica) &gt; original.
    /// </summary>
    private (string RutaArchivo, string NombreArchivo) ResolverArchivoDocumento(
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
            return (_fileStorageService.CombinarRuta(detalleFirmado.RutaDocumentoFirmado!, nombreFirmado), nombreFirmado);
        }

        if (!string.IsNullOrWhiteSpace(documento.RutaConTag) &&
            !string.IsNullOrWhiteSpace(documento.NombreConTag) &&
            !string.IsNullOrWhiteSpace(documento.ExtensionConTag))
        {
            string nombreConTag = $"{documento.NombreConTag}.{documento.ExtensionConTag}";
            return (_fileStorageService.CombinarRuta(documento.RutaConTag!, nombreConTag), nombreConTag);
        }

        if (inCodigo &&
            !string.IsNullOrWhiteSpace(documento.RutaCodigo) &&
            !string.IsNullOrWhiteSpace(documento.NombreCodigo) &&
            !string.IsNullOrWhiteSpace(documento.ExtensionCodigo))
        {
            string nombreCodigo = $"{documento.NombreCodigo}.{documento.ExtensionCodigo}";
            return (_fileStorageService.CombinarRuta(documento.RutaCodigo, nombreCodigo), nombreCodigo);
        }

        string nombreOriginal = $"{documento.NombreOriginal}.{documento.ExtensionOriginal}";
        return (_fileStorageService.CombinarRuta(documento.RutaOriginal, nombreOriginal), nombreOriginal);
    }

    private static string GenerarNombreUnicoEntry(
        HashSet<string> nombresUsados,
        int correlativo,
        string nombreOriginal)
    {
        if (nombresUsados.Add(nombreOriginal))
            return nombreOriginal;

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
}