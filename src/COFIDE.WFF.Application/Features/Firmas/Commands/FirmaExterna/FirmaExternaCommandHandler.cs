using AutoMapper;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Firmas.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Ports.Out;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;
namespace COFIDE.WFF.Application.Features.Firmas.Commands.FirmaExterna;

public sealed class FirmaExternaCommandHandler(
        IMapper mapper,
        ICurrentUserService currentUser,
        IDocumentoFirmadoValidator documentoFirmadoValidator,
        ISolicitudDocumentoRepository solicitudDocumentoRepository,
        IDetalleFirmaRepository detalleFirmaRepository,
        IFileStorageService fileStorageService,
        IPdfContentHashService pdfContentHashService
    ) : BaseCommandHandler<FirmaExternaCommand, string?>(mapper)
{
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly IDocumentoFirmadoValidator _documentoFirmadoValidator = documentoFirmadoValidator;
    private readonly ISolicitudDocumentoRepository _solicitudDocumentoRepository = solicitudDocumentoRepository;
    private readonly IDetalleFirmaRepository _detalleFirmaRepository = detalleFirmaRepository;
    private readonly IFileStorageService _fileStorageService = fileStorageService;
    private readonly IPdfContentHashService _pdfContentHashService = pdfContentHashService;

    public override async Task<string?> Handle(FirmaExternaCommand request, CancellationToken cancellationToken)
    {
        List<string> rutasCreadas = new();
        var documento = await _solicitudDocumentoRepository.ObtenerSolicitudDocumentoParaFirmadoInterno(request.IdSolicitudDocumento, cancellationToken, true);
        if (documento == null)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.DocumentoInvalid);

        string dniUsuario = _currentUser.UserDni!;
        string codigoUsuario = _currentUser.CodigoUsuario!;
        long idUSuario = _currentUser.UserIdLocal!.Value;


        string codigoSolicitud = documento.Solicitud.CodigoSolicitud;
        var documentoVigente = documento.ObtenerDocumentoVigenteParaFirma(documento.Solicitud.InCodigo);

        var existeDocumento = await _fileStorageService.ExistsFileAsync(
            documentoVigente.Ruta,
            documentoVigente.UltimoNombreOriginal,
            documentoVigente.ExtensionOriginal,
         cancellationToken);

        if (!existeDocumento)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.FirmaArchivoNotFound);

        // Extraer las firmas del documento en FileStorage
        string rutaCompleta = _fileStorageService.CombinarRuta(documentoVigente.Ruta, $"{documentoVigente.UltimoNombreOriginal}.{documentoVigente.ExtensionOriginal}");
        await using var stream = await _fileStorageService.GetFileAsync(rutaCompleta, cancellationToken);

        using var archivoFirmado = request.Documento.AbrirLectura();
        using var memoryStream = new MemoryStream();
        await archivoFirmado.CopyToAsync(memoryStream, cancellationToken);
        byte[] archivoBytes = memoryStream.ToArray();


        var detallefirma = await _solicitudDocumentoRepository.ObtenerDetalleFirmaPorDocumentoAsignado(request.IdSolicitudDocumento, idUSuario, cancellationToken);
        if (detallefirma == null)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.DetalleFirmaInvalid);

        detallefirma.ValidacionDocumentoFirmado();

        bool tieneDocumentoFirmado = documento.ContarFirmas() > 0;
        archivoFirmado.Position = 0;
        var resultado = await _documentoFirmadoValidator.Externo_ValidarDocumentoFirmadoAsync(stream, archivoFirmado, documentoVigente.Hash!, dniUsuario, documento.ObtenerUltimoDniFirmado(), tieneDocumentoFirmado);

        if (!resultado.EsValido)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.FirmaDocumentoInvalid.Format(resultado.Mensaje!));

        string subPathDocumentos = _fileStorageService.ObtenerCarpetaDocumentos(codigoSolicitud);
        string nombreDocumentoFirmado = documentoVigente!.NombreOriginalFirmado;
        string extensionDocumentoFirmado = documentoVigente!.ExtensionOriginal;
        string uniqueFileName = $"{documentoVigente!.NombreOriginalFirmado}.{documentoVigente!.ExtensionOriginal}";
        using var saveOriginalStream = new MemoryStream(archivoBytes);

        var rutaOriginal = await _fileStorageService.SaveFileAsync(
            saveOriginalStream,
            uniqueFileName,
            subPathDocumentos,
            uniqueFileName,
            cancellationToken);

        var hashFirmado = _pdfContentHashService.ObtenerHashContenido(archivoBytes);

        rutasCreadas.Add(rutaOriginal);
        detallefirma.Firmar(nombreDocumentoFirmado, extensionDocumentoFirmado, subPathDocumentos, hashFirmado, false);

        detallefirma.DocumentoFirmadoEvent(true, string.Empty, codigoUsuario);

        await _detalleFirmaRepository.UpdateAsync(detallefirma);

        return "";
    }
}
