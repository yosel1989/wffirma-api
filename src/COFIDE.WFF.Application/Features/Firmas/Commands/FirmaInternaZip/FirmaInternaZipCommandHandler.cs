using AutoMapper;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Firmas.DTOs;
using COFIDE.WFF.Application.Features.Firmas.Ports.Out;
using COFIDE.WFF.Application.Features.Firmas.Services;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;

namespace COFIDE.WFF.Application.Features.Firmas.Commands.FirmaInternaZip;

public class FirmaInternaZipCommandHandler(
    IMapper mapper,
    ICurrentUserService currentUser,
    ISolicitudReadPort solicitudRead,
    IFileStorageService fileStorageService,
    IZipStreamingService zipStreamingService,
    IFirmaPreparationService firmaPreparationService,
    IFirmaPort firmaPort,
    IOrientacionPaginaPdfService orientacionPaginaPdfService
) : BaseCommandHandler<FirmaInternaZipCommand, FirmaInternaResponseDto?>(mapper)
{
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly ISolicitudReadPort _solicitudRead = solicitudRead;
    private readonly IFileStorageService _fileStorageService = fileStorageService;
    private readonly IZipStreamingService _zipStreamingService = zipStreamingService;
    private readonly IFirmaPreparationService _firmaPreparationService = firmaPreparationService;
    private readonly IFirmaPort _firmaPort = firmaPort;
    private readonly IOrientacionPaginaPdfService _orientacionPaginaPdfService =
        orientacionPaginaPdfService;

    public override async Task<FirmaInternaResponseDto?> Handle(
        FirmaInternaZipCommand request,
        CancellationToken cancellationToken)
    {
        var documentos =
            await _solicitudRead.ObtenerDocumentosParaFirmaInternaMasivaRead(
                request.IdSolicitud,
                request.Documentos,
                cancellationToken,
                true);

        if (documentos.Count == 0)
        {
            throw new ValidacionAplicacion.ApplicationException(
                ApplicationErrors.DocumentosNotFound);
        }

        long idUsuario = _currentUser.UserIdLocal!.Value;

        var primerDocumento = documentos.First();

        var primerDocumentoPorFirmar =
            primerDocumento.ObtenerDocumentoVigenteParaFirma(
                primerDocumento.Solicitud.InCodigo);

        string rutaPrimerDocumento =
            _fileStorageService.CombinarRuta(
                primerDocumentoPorFirmar.Ruta,
                $"{primerDocumentoPorFirmar.UltimoNombreOriginal}." +
                $"{primerDocumentoPorFirmar.ExtensionOriginal}");

        await using var streamPrimerDocumento =
            await _fileStorageService.GetFileAsync(
                rutaPrimerDocumento,
                cancellationToken)
            ?? throw new ValidacionAplicacion.ApplicationException(
                ApplicationErrors.ArchivoNotFound);

        streamPrimerDocumento.Position = 0;

        var informacionPrimerDocumento =
            _orientacionPaginaPdfService.ObtenerPrimeraPagina(
                streamPrimerDocumento);

        var firmaRequest =
            await _firmaPreparationService.PrepararDocumentoFirmaAsync(
                primerDocumento,
                primerDocumento.Solicitud.CodigoSolicitud,
                idUsuario,
                documentos.Count,
                informacionPrimerDocumento,
                cancellationToken);

        var documentosLote = documentos;
        var informacionPaginaLote = informacionPrimerDocumento;
    
        if (firmaRequest.EsVisador && documentos.Count > 1)
        {
            var informacionPorDocumento =
                new Dictionary<long, InformacionPaginaPdfDto>
                {
                    [primerDocumento.Id] = informacionPrimerDocumento
                };

            foreach (var documento in documentos.Skip(1))
            {
                var documentoPorFirmar =
                    documento.ObtenerDocumentoVigenteParaFirma(
                        documento.Solicitud.InCodigo);

                string rutaDocumento =
                    _fileStorageService.CombinarRuta(
                        documentoPorFirmar.Ruta,
                        $"{documentoPorFirmar.UltimoNombreOriginal}." +
                        $"{documentoPorFirmar.ExtensionOriginal}");

                await using var streamDocumento =
                    await _fileStorageService.GetFileAsync(
                        rutaDocumento,
                        cancellationToken)
                    ?? throw new ValidacionAplicacion.ApplicationException(
                        ApplicationErrors.ArchivoNotFound);

                streamDocumento.Position = 0;

                informacionPorDocumento[documento.Id] =
                    _orientacionPaginaPdfService.ObtenerPrimeraPagina(
                        streamDocumento);
            }

            /*
             * se procesa la orientación correspondiente al primer documento recibido.
             */
            bool esHorizontalPrimerLote =
                informacionPrimerDocumento.EsHorizontal;

            documentosLote = documentos
                .Where(documento =>
                    informacionPorDocumento[documento.Id].EsHorizontal ==
                    esHorizontalPrimerLote)
                .ToList();

            var documentoReferencia = documentosLote.First();

            informacionPaginaLote =
                informacionPorDocumento[documentoReferencia.Id];

            /*
             * Si se excluyeron documentos de otra orientación.
             */
            if (documentosLote.Count != documentos.Count)
            {
                firmaRequest =
                    await _firmaPreparationService.PrepararDocumentoFirmaAsync(
                        documentoReferencia,
                        documentoReferencia.Solicitud.CodigoSolicitud,
                        idUsuario,
                        documentosLote.Count,
                        informacionPaginaLote,
                        cancellationToken);
            }
        }

        var archivosZip = new List<FirmaZipRequestDto>();

        await using var zipArchivo = new MemoryStream();

        try
        {
            foreach (var documento in documentosLote)
            {
                var documentoPorFirmar =
                    documento.ObtenerDocumentoVigenteParaFirma(
                        documento.Solicitud.InCodigo);

                var detalleFirma =
                    documento.DetallesFirma.FirstOrDefault(detalle =>
                        detalle.IdSolicitudDocumento == documento.Id &&
                        detalle.IdFirmanteDocumento == idUsuario)
                    ?? throw new ValidacionAplicacion.ApplicationException(
                        ApplicationErrors.DetalleFirmaInvalid);

                detalleFirma.ValidacionDocumentoFirmado();

                string rutaCompleta =
                    _fileStorageService.CombinarRuta(
                        documentoPorFirmar.Ruta,
                        $"{documentoPorFirmar.UltimoNombreOriginal}." +
                        $"{documentoPorFirmar.ExtensionOriginal}");

                await using var stream =
                    await _fileStorageService.GetFileAsync(
                        rutaCompleta,
                        cancellationToken)
                    ?? throw new ValidacionAplicacion.ApplicationException(
                        ApplicationErrors.ArchivoNotFound);

                var memoryStream = new MemoryStream();

                await stream.CopyToAsync(
                    memoryStream,
                    cancellationToken);

                memoryStream.Position = 0;

                archivosZip.Add(new FirmaZipRequestDto
                {
                    FileName = $"{detalleFirma.CodigoReferencial}.pdf",
                    Content = memoryStream
                });

                /*
                 * Solo se bloquean los documentos pertenecientes
                 * a la orientación procesada en esta llamada.
                 */
                detalleFirma.Bloquear();
                detalleFirma.FechaInicioFechaFin(
                    firmaRequest.TiempoEspera);
            }

            await _zipStreamingService.StreamZipAsync(
                zipArchivo,
                archivosZip,
                cancellationToken);

            zipArchivo.Position = 0;
            firmaRequest.ArchivoStream = zipArchivo;

            var resultado =
                await _firmaPort.FirmarBatchAsync(
                    firmaRequest,
                    cancellationToken);

            resultado.TiempoEspera =
                firmaRequest.TiempoEspera;

            /*
             * El código de BigSigner se asigna a los documentos del lote actual.
             */
            foreach (var documento in documentosLote)
            {
                var detalleFirma =
                    documento.DetallesFirma.FirstOrDefault(detalle =>
                        detalle.IdSolicitudDocumento == documento.Id &&
                        detalle.IdFirmanteDocumento == idUsuario)
                    ?? throw new ValidacionAplicacion.ApplicationException(
                        ApplicationErrors.DetalleFirmaInvalid);

                detalleFirma.SetearCodigoFirma(
                    resultado.CodigoProceso!);
            }

            var proceso = new FirmaInternaProcesoDto
            {
                Orientacion = informacionPaginaLote.EsHorizontal
                    ? "Horizontal"
                    : "Vertical",

                Documentos = documentosLote
                    .Select(documento => documento.Id)
                    .ToList(),

                UrlFirma = resultado.UrlFirma,
                CodigoProceso = resultado.CodigoProceso,
                TiempoEspera = firmaRequest.TiempoEspera
            };

            return new FirmaInternaResponseDto
            {
                UrlFirma = resultado.UrlFirma,
                CodigoProceso = resultado.CodigoProceso,
                TiempoEspera = firmaRequest.TiempoEspera,

                /*
                 * Se devuelve el lote
                 */
                Procesos = [proceso]
            };
        }
        finally
        {
            foreach (var archivo in archivosZip)
            {
                await archivo.Content.DisposeAsync();
            }
        }
    }
}