using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Features.Firmas.DTOs;
using COFIDE.WFF.Application.Features.Firmas.Ports.Out;
using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.DTOs;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Inputs;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Vo;
using COFIDE.WFF.Application.Common.Files;
using System.Text.Json;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;
namespace COFIDE.WFF.Application.Features.Solicitudes.Services;

public class SolicitudDocumentoService(
        IFileStorageService fileStorageService,
        IPdfCodigoInserter pdfCodigoInserter,
        IPdfContentHashService pdfContentHashService,
        IDocumentoFirmadoValidator documentoFirmadoValidator,
        IParametroRepository parametroRepository
    ) : ISolicitudDocumentoService
{
    private readonly IFileStorageService _fileStorageService = fileStorageService;
    private readonly IPdfCodigoInserter _pdfCodigoInserter = pdfCodigoInserter;
    private readonly IPdfContentHashService _pdfContentHashService = pdfContentHashService;
    private readonly IDocumentoFirmadoValidator _documentoFirmadoValidator = documentoFirmadoValidator;

    private readonly IParametroRepository _parametroRepository = parametroRepository;
    private readonly string sufijoDocumentoTag = "[T]";
    public async Task<IReadOnlyCollection<DocumentoProcesadoInput>> CrearDocumentoInput(List<ArchivoEntrada>? archivos,
        string codigoSolicitud, bool inCodigo, string? documentosTag, CancellationToken cancellationToken)
    {
        if (archivos == null || archivos.Count == 0)
            return [];

        var lista = new List<DocumentoProcesadoInput>();

        foreach (var file in archivos)
        {
            await using var stream = file.AbrirLectura();

            lista.Add(await ProcesarArchivoAsync(
                stream,
                file.Nombre,
                codigoSolicitud,
                inCodigo,
                documentosTag,
                cancellationToken));
        }

        return lista;
    }

    public async Task<IReadOnlyCollection<DocumentoProcesadoInput>> CrearDocumentoInputDesdeStorage(IReadOnlyCollection<SolicitudDocumento> documentos, List<long>? idsDocumentosConservar,
        string codigoSolicitud, bool inCodigo, CancellationToken cancellationToken)
    {
        idsDocumentosConservar ??= [];

        var lista = new List<DocumentoProcesadoInput>();

        foreach (var doc in documentos
            .Where(x => x.Estado && idsDocumentosConservar.Contains(x.Id)))
        {
            string fileName = $"{doc.NombreOriginal}.{doc.ExtensionOriginal}";
            string path = _fileStorageService.CombinarRuta(doc.RutaOriginal, fileName);

            await using var stream =
                await _fileStorageService.GetFileAsync(path, cancellationToken);

            lista.Add(await ProcesarArchivoAsync(
                stream,
                fileName,
                codigoSolicitud,
                inCodigo,
                "",
                cancellationToken));
        }

        return lista;
    }

    public async Task SincronizarCambioCodigoAsync(Solicitud solicitud, bool nuevoInCodigo, CancellationToken cancellationToken)
    {
        if (solicitud.InCodigo == nuevoInCodigo)
            return;

        var documentos = solicitud.Documentos
            .Where(x => x.Estado)
            .ToList();

        if (documentos.Count == 0)
            return;

        if (!nuevoInCodigo)
        {
            await QuitarCodigoAsync(documentos, cancellationToken);
            return;
        }

        await GenerarCodigoAsync(
            documentos,
            solicitud.CodigoSolicitud,
            cancellationToken);
    }

    public async Task SincronizarTagDocumentoAsync(Solicitud solicitud, CancellationToken cancellationToken)
    {
        var config = await _parametroRepository.ConfiguracionFirmaAsync(cancellationToken);
        string subPath = _fileStorageService.ObtenerCarpetaDocumentos(solicitud.CodigoSolicitud);
        var documentos = solicitud.Documentos
            .Where(x => x.Estado)
            .ToList();

        if (documentos.Count == 0)
            return;


        foreach (var doc in solicitud.Documentos)
        {
            await ProcesarDocumentoTagAsync(
                    solicitud,
                    doc,
                    config,
                    subPath,
                    cancellationToken);
        }

    }


    #region METODOS PRIVADOS

    private async Task ProcesarDocumentoTagAsync(
    Solicitud solicitud,
    SolicitudDocumento doc,
    ConfiguracionFirmaDto config,
    string subPath,
    CancellationToken cancellationToken)
    {
        string? nombreCodigo = null;
        string? extensionCodigo = null;
        string? hashCodigo = null;
        string? rutaCodigo = null;

        await EliminarDocumentoTagActualAsync(doc, cancellationToken);

        if (doc.DocumentoTags.Any(x => x.Estado))
        {
            var documentoVigente = doc.ObtenerDatosDocumentoParaTag(solicitud.InCodigo);
            string fileName = $"{documentoVigente.UltimoNombreOriginal}.{documentoVigente.ExtensionOriginal}";
            string path = _fileStorageService.CombinarRuta(documentoVigente.Ruta, fileName);

            await using var stream = await _fileStorageService.GetFileAsync(path, cancellationToken);

            var nombreEsperadoTag = $"{documentoVigente.UltimoNombreOriginal}_{sufijoDocumentoTag}";
            if (doc.TieneRutaTag() && !string.Equals(doc.NombreConTag, nombreEsperadoTag, StringComparison.OrdinalIgnoreCase))
            {
                await _fileStorageService.DeleteFileAsync(
                    _fileStorageService.CombinarRuta(
                        doc.RutaConTag!,
                        $"{doc.NombreConTag}.{doc.ExtensionConTag}"),
                    cancellationToken);

                doc.LimpiarDocumentoConTag();
            }


            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms, cancellationToken);

            byte[] fileBytes = ms.ToArray();

            byte[] documentoBaseParaTags = fileBytes;

            // Verificar si el archivo tiene código
            if (solicitud.InCodigo)
            {

                documentoBaseParaTags = _pdfCodigoInserter.InsertCodigo(fileBytes, solicitud.CodigoSolicitud);

                using var codedStream = new MemoryStream(documentoBaseParaTags);

                await _fileStorageService.SaveFileAsync(
                    codedStream,
                    documentoVigente.UltimoNombreOriginal,
                    subPath,
                    documentoVigente.UltimoNombreOriginal,
                    cancellationToken);

                nombreCodigo = documentoVigente.UltimoNombreOriginal;
                extensionCodigo = documentoVigente.ExtensionOriginal;
                hashCodigo = _pdfContentHashService.ObtenerHashContenido(documentoBaseParaTags);
                rutaCodigo = subPath;

                doc.AsignarDocumentoCodigo(nombreCodigo, extensionCodigo, rutaCodigo, hashCodigo);
            }                       

            var pdfConTags = documentoBaseParaTags;
            foreach (var coord in doc.DocumentoTags.Where(x => x.Estado))
            {
                pdfConTags = _pdfCodigoInserter.InsertTag(
                pdfConTags,
                config.NombreTag,
                coord.NumeroPagina,
                (double)coord.UbicacionX,
                (double)coord.UbicacionY,
                config.ColorEtiqueta,
                config.FirmaMargenAlto,
                cancellationToken);
            }            

            await GuardarDocumentoConTagsAsync(
                  doc,
                  documentoVigente,
                  pdfConTags,
                  subPath,
                  cancellationToken);

        }
    }


    private async Task GuardarDocumentoConTagsAsync(
    SolicitudDocumento documento,
    DocumentoVigente documentoVigente,
    byte[] pdfConTags,
    string subPath,
    CancellationToken cancellationToken)
    {
        string nombreConTag =
            documento.TieneRutaTag()
                ? documento.NombreConTag!
                : $"{documentoVigente.UltimoNombreOriginal}_{sufijoDocumentoTag}";


        string fileName =
            $"{nombreConTag}.{documentoVigente.ExtensionOriginal}";


        await using var stream =
            new MemoryStream(pdfConTags);


        await _fileStorageService.SaveFileAsync(
            stream,
            fileName,
            subPath,
            fileName,
            cancellationToken);


        documento.AsignarDocumentoConTag(
            nombreConTag,
            documentoVigente.ExtensionOriginal,
            documentoVigente.Ruta,
            _pdfContentHashService.ObtenerHashContenido(pdfConTags));
    }


    private async Task EliminarDocumentoTagActualAsync(SolicitudDocumento doc, CancellationToken cancellationToken)
    {
        if (!doc.TieneRutaTag())
            return;

        await _fileStorageService.DeleteFileAsync(
            _fileStorageService.CombinarRuta(
                doc.RutaConTag!,
                $"{doc.NombreConTag}.{doc.ExtensionConTag}"),
            cancellationToken);

        doc.LimpiarDocumentoConTag();
    }

    private async Task<DocumentoProcesadoInput> ProcesarArchivoAsync(Stream sourceStream, string fileName, string codigoSolicitud,
        bool inCodigo, string? documentosTag, CancellationToken cancellationToken)
    {
        using var ms = new MemoryStream();
        await sourceStream.CopyToAsync(ms, cancellationToken);

        var fileBytes = ms.ToArray();

        if (fileBytes.Length == 0)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.DocumentoEmpty);

        string subPath = _fileStorageService.ObtenerCarpetaDocumentos(codigoSolicitud);
        string hashOriginal = _pdfContentHashService.ObtenerHashContenido(fileBytes);
        string nombreOriginal = Path.GetFileNameWithoutExtension(fileName);
        string extensionOriginal = Path.GetExtension(fileName).TrimStart('.');

        using var saveOriginal = new MemoryStream(fileBytes);

        await _fileStorageService.SaveFileAsync(
            saveOriginal,
            fileName,
            subPath,
            fileName,
            cancellationToken);

        string? nombreCodigo = null;
        string? extensionCodigo = null;
        string? hashCodigo = null;
        string? rutaCodigo = null;

        string? nombreConTag = null;
        string? extensionConTag = null;
        string? hashConTag = null;
        string? rutaConTag = null;

        var tags = ObtenerTagsConfiguracion(documentosTag);
        byte[] documentoBaseParaTags = fileBytes;
        bool tieneFirma;

        using (var validarStream = new MemoryStream(fileBytes))
        {
            tieneFirma =
                await _documentoFirmadoValidator
                    .TieneFirmaAsync(validarStream);
        }

        if (tieneFirma)
        {
            inCodigo = false;
            tags = [];

            nombreCodigo = nombreOriginal;
            extensionCodigo = extensionOriginal;
            hashCodigo = hashOriginal;
            rutaCodigo = subPath;
        }

        if (inCodigo)
        {
            documentoBaseParaTags = _pdfCodigoInserter.InsertCodigo(fileBytes, codigoSolicitud);

            string codedFileName = $"{nombreOriginal}-COD.{extensionOriginal}";

            using var codedStream = new MemoryStream(documentoBaseParaTags);

            await _fileStorageService.SaveFileAsync(
                codedStream,
                codedFileName,
                subPath,
                codedFileName,
                cancellationToken);

            nombreCodigo = $"{nombreOriginal}-COD";
            extensionCodigo = extensionOriginal;
            hashCodigo = _pdfContentHashService.ObtenerHashContenido(documentoBaseParaTags);
            rutaCodigo = subPath;
        }


        var documentoProcesadoInput = new DocumentoProcesadoInput
        {
            NombreOriginal = nombreOriginal,
            ExtensionOriginal = extensionOriginal,
            RutaOriginal = subPath,
            HashOriginal = hashOriginal,
            NombreCodigo = nombreCodigo,
            ExtensionCodigo = extensionCodigo,
            RutaCodigo = rutaCodigo,
            HashCodigo = hashCodigo,
            NombreConTag = nombreConTag,
            ExtensionConTag = extensionConTag,
            RutaConTag = rutaConTag,
            HashConTag = hashConTag,
            PesoDocumento = fileBytes.LongLength
        };

        // Validar existencia de firma en el documento
        var TieneFirmaDigital = await _documentoFirmadoValidator.TieneFirmaAsync(sourceStream);
        if (TieneFirmaDigital)
            return documentoProcesadoInput;

        var tagsDocumento = tags
    .Where(t =>
        ObtenerNombreSinExtension(t.NombreDocumento)
            .Equals(
                ObtenerNombreSinExtension(nombreOriginal),
                StringComparison.OrdinalIgnoreCase))
    .ToList();

        return await AgregarTag(inCodigo, subPath, documentoProcesadoInput, documentoBaseParaTags, tagsDocumento, cancellationToken);
    }
    private async Task QuitarCodigoAsync(List<SolicitudDocumento> docs, CancellationToken cancellationToken)
    {
        foreach (var doc in docs)
        {
            if (!string.IsNullOrWhiteSpace(doc.RutaCodigo) && (doc.NombreCodigo != doc.NombreOriginal))
            {
                var path = _fileStorageService.CombinarRuta(
                    doc.RutaCodigo!,
                    $"{doc.NombreCodigo}.{doc.ExtensionCodigo}");

                await _fileStorageService.DeleteFileAsync(path, cancellationToken);
            }

            doc.RemoverDocumentoCodigo();
        }
    }
    private async Task GenerarCodigoAsync(List<SolicitudDocumento> documentos, string codigoSolicitud, CancellationToken cancellationToken)
    {

        string subPathDocumentos = _fileStorageService.ObtenerCarpetaDocumentos(codigoSolicitud);

        foreach (var doc in documentos)
        {
            var pathOriginal = _fileStorageService.CombinarRuta(doc.RutaOriginal, $"{doc.NombreOriginal}.{doc.ExtensionOriginal}");

            await using var originalStream = await _fileStorageService.GetFileAsync(pathOriginal, cancellationToken);
            using var ms = new MemoryStream();
            await originalStream.CopyToAsync(ms, cancellationToken);

            var bytes = ms.ToArray();

            bool tieneFirma;
            using (var validarStream = new MemoryStream(bytes))
            {
                tieneFirma =
                    await _documentoFirmadoValidator
                        .TieneFirmaAsync(validarStream);
            }


            if (tieneFirma)
            {
                doc.AsignarDocumentoCodigo(doc.NombreOriginal, doc.ExtensionOriginal, doc.RutaOriginal, doc.HashOriginal);
            }
            else
            {


                var codedBytes = _pdfCodigoInserter.InsertCodigo(bytes, codigoSolicitud);
                var fileName = $"{doc.NombreOriginal}-COD.{doc.ExtensionOriginal}";
                using var codedStream = new MemoryStream(codedBytes);

                await _fileStorageService.SaveFileAsync(
                       codedStream,
                       fileName,
                       subPathDocumentos,
                       fileName,
                       cancellationToken);

                var hash = _pdfContentHashService.ObtenerHashContenido(codedBytes);
                doc.AsignarDocumentoCodigo(Path.GetFileNameWithoutExtension(fileName), doc.ExtensionOriginal, subPathDocumentos, hash);
            }


        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    private static List<DocumentoTagRequestDto> ObtenerTagsConfiguracion(string? documentosTag)
    {
        if (string.IsNullOrWhiteSpace(documentosTag))
            return [];

        try
        {
            return JsonSerializer.Deserialize<List<DocumentoTagRequestDto>>(
                documentosTag,
                JsonOptions)
            ?? throw new ValidacionAplicacion.ApplicationException(
                ApplicationErrors.ConfiguracionInvalid);
        }
        catch (JsonException)
        {
            throw new ValidacionAplicacion.ApplicationException(
                ApplicationErrors.ConfiguracionInvalid);
        }
    }

    public async Task<DocumentoProcesadoInput> AgregarTag(
    bool inCodigo,
    string subPath,
    DocumentoProcesadoInput request,
    byte[] documentoBase,
    List<DocumentoTagRequestDto> tags,
    CancellationToken cancellationToken)
    {
        if (tags.Count == 0)
            return request;

        var config = await _parametroRepository.ConfiguracionFirmaAsync(cancellationToken);

        var pdfConTags = documentoBase;

        foreach (var tag in tags)
        {
            foreach (var coord in tag.Coordenadas)
            {
                pdfConTags = _pdfCodigoInserter.InsertTag(
                    pdfConTags,
                    config.NombreTag,
                    coord.NumeroPagina,
                    (double)coord.X,
                    (double)coord.Y,
                    config.ColorEtiqueta,
                    config.FirmaMargenAlto,
                    cancellationToken);
            }
        }

        var nombreArchivoConTAG = inCodigo
            ? $"{request.NombreCodigo}_{sufijoDocumentoTag}"
            : $"{request.NombreOriginal}_{sufijoDocumentoTag}";

        var codedFileName = $"{nombreArchivoConTAG}.{request.ExtensionOriginal}";

        using var tagStream = new MemoryStream(pdfConTags);

        await _fileStorageService.SaveFileAsync(
            tagStream,
            codedFileName,
            subPath,
            codedFileName,
            cancellationToken);

        request.NombreConTag = nombreArchivoConTAG;
        request.ExtensionConTag = request.ExtensionOriginal;
        request.HashConTag = _pdfContentHashService.ObtenerHashContenido(pdfConTags);
        request.RutaConTag = subPath;

        return request;
    }

    private static string ObtenerNombreSinExtension(string nombre)
    {
        return Path.GetFileNameWithoutExtension(nombre);
    }

    #endregion
}
