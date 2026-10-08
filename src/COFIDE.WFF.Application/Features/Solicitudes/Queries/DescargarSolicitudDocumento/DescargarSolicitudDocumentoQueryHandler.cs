using AutoMapper;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Common.Ports.Out;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;
namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.DescargarSolicitudDocumento;

public sealed class DescargarSolicitudDocumentoHandler(
        IMapper mapper,
        IFileStorageService fileStorageService,
        ISolicitudRepository solicitudRepository
    )
    : BaseQueryHandler<DescargarSolicitudDocumentoQuery, DescargarSolicitudDocumentoDto>(mapper)
{
    private readonly IFileStorageService _fileStorageService = fileStorageService;
    private readonly ISolicitudRepository _solicitudRepository = solicitudRepository;

    public override async Task<DescargarSolicitudDocumentoDto> Handle(
        DescargarSolicitudDocumentoQuery request,
        CancellationToken cancellationToken)
    {
        var solicitudDocumento = await _solicitudRepository
            .ObtenerSolicitudDocumentoPorId(
                request.IdSolicitudDocumento,
                cancellationToken,
                tracking: false);

        if (solicitudDocumento is null)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.DocumentoVacio);

        string ruta;
        string nombre;
        string extension;

        var detalleFirmado = solicitudDocumento.DetallesFirma?
            .Where(df =>
                df.FechaHoraFirma != null &&
                !string.IsNullOrWhiteSpace(df.RutaDocumentoFirmado) &&
                !string.IsNullOrWhiteSpace(df.NombreDocumentoFirmado) &&
                !string.IsNullOrWhiteSpace(df.ExtensionDocumentoFirmado))
            .OrderByDescending(df => df.NombreDocumentoFirmado!.Length)
            .FirstOrDefault();

        if (detalleFirmado is not null)
        {
            ruta = _fileStorageService.CombinarRuta(
                detalleFirmado.RutaDocumentoFirmado!,
                $"{detalleFirmado.NombreDocumentoFirmado}.{detalleFirmado.ExtensionDocumentoFirmado}");

            nombre = detalleFirmado.NombreDocumentoFirmado!;
            extension = detalleFirmado.ExtensionDocumentoFirmado!;
        }
        else if (!string.IsNullOrWhiteSpace(solicitudDocumento.RutaConTag) &&
            !string.IsNullOrWhiteSpace(solicitudDocumento.NombreConTag) &&
            !string.IsNullOrWhiteSpace(solicitudDocumento.ExtensionConTag))
        {
            ruta = _fileStorageService.CombinarRuta(
                solicitudDocumento.RutaConTag,
                $"{solicitudDocumento.NombreConTag}.{solicitudDocumento.ExtensionConTag}");

            nombre = solicitudDocumento.NombreConTag;
            extension = solicitudDocumento.ExtensionConTag;
        }
        else if (solicitudDocumento.Solicitud.InCodigo)
        {
            ValidarArchivo(
                   solicitudDocumento.RutaCodigo,
                   solicitudDocumento.NombreCodigo,
                   solicitudDocumento.ExtensionCodigo);

            ruta = _fileStorageService.CombinarRuta(
                solicitudDocumento.RutaCodigo!,
                $"{solicitudDocumento.NombreCodigo}.{solicitudDocumento.ExtensionCodigo}");

            nombre = solicitudDocumento.NombreCodigo!;
            extension = solicitudDocumento.ExtensionCodigo!;
        }
        else
        {
            ValidarArchivo(
               solicitudDocumento.RutaOriginal,
               solicitudDocumento.NombreOriginal,
               solicitudDocumento.ExtensionOriginal);

            ruta = _fileStorageService.CombinarRuta(
                solicitudDocumento.RutaOriginal,
                $"{solicitudDocumento.NombreOriginal}.{solicitudDocumento.ExtensionOriginal}");

            nombre = solicitudDocumento.NombreOriginal;
            extension = solicitudDocumento.ExtensionOriginal;
        }

        await using var stream = await _fileStorageService.GetFileAsync(ruta, cancellationToken);

        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, cancellationToken);

        var archivoBase64 = Convert.ToBase64String(memoryStream.ToArray());

        return new DescargarSolicitudDocumentoDto
        {
            Nombre = nombre,
            Extension = extension,
            ArchivoBase64 = archivoBase64
        };
    }

    private static void ValidarArchivo(
    string? ruta,
    string? nombre,
    string? extension)
    {
        if (string.IsNullOrWhiteSpace(ruta) ||
            string.IsNullOrWhiteSpace(nombre) ||
            string.IsNullOrWhiteSpace(extension))
        {
            throw new ValidacionAplicacion.ApplicationException(
                ApplicationErrors.ValidationFailed);
        }
    }
}