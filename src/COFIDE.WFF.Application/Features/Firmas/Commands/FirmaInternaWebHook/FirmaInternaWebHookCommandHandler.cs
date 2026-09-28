using AutoMapper;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Firmas.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Ports.Out;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;
namespace COFIDE.WFF.Application.Features.Firmas.Commands.FirmaInternaWebHook;

public class FirmaInternaWebHookCommandHandler(
        IMapper mapper,
        ICurrentUserService currentUser,
        IDocumentoFirmadoValidator documentoFirmadoValidator,
        IFirmaPort firmaPort,
        IDetalleFirmaRepository detalleFirmaRepository,
        IFileStorageService fileStorageService,
        IPdfContentHashService pdfContentHashService,
        ILogger<FirmaInternaWebHookCommandHandler> logger
    ) : BaseCommandHandler<FirmaInternaWebHookCommand, string?>(mapper)
{
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly IDocumentoFirmadoValidator _documentoFirmadoValidator = documentoFirmadoValidator;
    private readonly IFirmaPort _firmaPort = firmaPort;
    private readonly IDetalleFirmaRepository _detalleFirmaRepository = detalleFirmaRepository;
    private readonly IFileStorageService _fileStorageService = fileStorageService;
    private readonly IPdfContentHashService _pdfContentHashService = pdfContentHashService;
    private readonly ILogger<FirmaInternaWebHookCommandHandler> _logger = logger;


    public override async Task<string?> Handle(
      FirmaInternaWebHookCommand request,
      CancellationToken cancellationToken)
    {
        _logger.LogInformation("Webhook recibido desde API-FIRD Code={CodigoUsuario}-{Code}", _currentUser.CodigoUsuario, request.Code);

        var detalles = await _detalleFirmaRepository.ObtenerDetalleFirmaPorCodigoFirma(request.Code, cancellationToken);
        if (detalles == null || detalles.Count == 0)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.DetalleFirmaInvalid);

        var documentos = JsonSerializer.Deserialize<List<DocumentoFirmadoDto>>(request.DocumentId ?? "[]");
        if (documentos == null || documentos.Count == 0)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.DocumentoInvalid);

        foreach (var documento in documentos)
        {

            _logger.LogInformation("Usuario:{CodigoUsuario}", _currentUser.CodigoUsuario);
            _logger.LogInformation("documento={CodigoUsuario}-{DocumentName}", _currentUser.CodigoUsuario, documento.documentName);
            var detalleFirma = detalles.FirstOrDefault(x => ObtenerNombreSinExtension(x.CodigoReferencial!) == ObtenerNombreSinExtension(documento.documentName));

            if (detalleFirma is null)
                continue;

            if (detalleFirma.EstadoDocumento.Codigo == 3)
            {
                _logger.LogInformation("Documento ya firmado. Ignorado Id={Id}", detalleFirma.Id);
                continue;
            }

            try
            {
                await ProcesarDocumentoAsync(                   
                   detalleFirma,
                   documento,
                   request.Code,
                   _currentUser.CodigoUsuario!,
                   cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error procesando firma IdDetalleFirma={Id}",
                    detalleFirma.Id);

                detalleFirma.ActivarDocumento();

                await _detalleFirmaRepository
                    .UpdateAsync(detalleFirma);
            }
        }
        return "OK";
    }

    private static string ObtenerNombreSinExtension(string nombre)
    {
        return Path.GetFileNameWithoutExtension(nombre);
    }
     
     private async Task ProcesarDocumentoAsync(
        dynamic detalleFirma,
        DocumentoFirmadoDto documento,
        string code,
        string codigoUsuario,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("entro a ProcesarDocumentoAsync");
        var bytes = await _firmaPort.ObtenerDocumentoFirmadoAsync(detalleFirma.UsuarioCreacion, code, documento.id, cancellationToken);

        if (bytes is null || bytes.Length == 0)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.DocumentoFirmadoEmpty);

        string codigoSolicitud =
            detalleFirma.SolicitudDocumento
                .Solicitud.CodigoSolicitud;

        string subPath = Path.Combine(codigoSolicitud, "DOCUMENTOS");

        var vigente = detalleFirma
            .SolicitudDocumento
            .ObtenerDocumentoVigenteParaFirma(
                detalleFirma.SolicitudDocumento.Solicitud.InCodigo);

        string nombre = vigente.NombreOriginalFirmado;

        string extension = vigente.ExtensionOriginal;

        string uniqueFile = $"{nombre}.{extension}";

        using var uploadStream = new MemoryStream(bytes);

        string rutaCompleta = Path.Combine(vigente.Ruta, $"{vigente.UltimoNombreOriginal}.{vigente.ExtensionOriginal}");
        await using var streamOriginal = await _fileStorageService.GetFileAsync(rutaCompleta, cancellationToken);
        var resultadoValidacion = await _documentoFirmadoValidator.Interno_ValidarDocumentoFirmadoAsync(streamOriginal, uploadStream);
        _logger.LogInformation("resultadoValidacion:{ResultadoValidacion}", resultadoValidacion);
        _logger.LogInformation("codigoUsuario:{CodigoUsuario}", codigoUsuario);
        if (!string.IsNullOrWhiteSpace(resultadoValidacion))
        {
            detalleFirma.DocumentoFirmadoEvent(false, "No es posible firmar esta versión del documento porque ha sido actualizada por otro firmante. Vuelva a firmar en unos minutos.", codigoUsuario);
            detalleFirma.ActivarDocumento();

            await _detalleFirmaRepository
                .UpdateAsync(detalleFirma);
        }
        else
        {

            _logger.LogDebug("entro a SaveFileAsync");
            uploadStream.Position = 0;
            await _fileStorageService.SaveFileAsync(
                 uploadStream,
                 uniqueFile,
                 subPath,
                 uniqueFile,
                 cancellationToken);

            var hashFirmado = _pdfContentHashService.ObtenerHashContenido(bytes);

            detalleFirma.Firmar(
                nombre,
                extension,
                subPath,
                hashFirmado,
                true);


            detalleFirma.DocumentoFirmadoEvent(true, string.Empty, codigoUsuario);

            await _detalleFirmaRepository
                .UpdateAsync(detalleFirma);
        }

    }
     

}
