using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Firmas.Commands.FirmaInternaZip;
using COFIDE.WFF.Application.Features.Firmas.DTOs;
using COFIDE.WFF.Application.Features.Firmas.Ports.In;
using COFIDE.WFF.Application.Features.Firmas.Ports.Out;
using COFIDE.WFF.Application.Features.Firmas.Services;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Ports.Out;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;
namespace COFIDE.WFF.Application.Features.Firmas.UseCases;

public class FirmaInternaZipUseCase(
        ICurrentUserService currentUser,
        ISolicitudReadPort solicitudRead,
        IFileStorageService fileStorageService,
        IZipStreamingService zipStreamingService,
        IFirmaPreparationService firmaPreparationService,
        IFirmaPort firmaPort,
        IOrientacionPaginaPdfService orientacionPaginaPdfService
    ) : IFirmaInternaZipUseCase
{
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly ISolicitudReadPort _solicitudRead = solicitudRead;
    private readonly IFileStorageService _fileStorageService = fileStorageService;
    private readonly IZipStreamingService _zipStreamingService = zipStreamingService;
    private readonly IFirmaPreparationService _firmaPreparationService = firmaPreparationService;
    private readonly IFirmaPort _firmaPort = firmaPort;
    private readonly IOrientacionPaginaPdfService _orientacionPaginaPdfService = orientacionPaginaPdfService;

    public async Task<FirmaInternaResponseDto?> Firmar(FirmaInternaZipCommand request, CancellationToken cancellationToken)
    {
        var documentos = await _solicitudRead.ObtenerDocumentosParaFirmaInternaMasivaRead(request.IdSolicitud, request.Documentos, cancellationToken, true);
        if (documentos.Count == 0)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.DocumentosNotFound);

        long idUsuario = _currentUser.UserIdLocal!.Value;
        var archivosZip = new List<FirmaZipRequestDto>();
        await using var zipArchivo = new MemoryStream();

        try
        {
            //var firmaRequest = await _firmaPreparationService.PrepararDocumentoFirmaAsync(documentos.FirstOrDefault()!,
            //    documentos.FirstOrDefault()!.Solicitud.CodigoSolicitud, idUsuario, documentos.Count, cancellationToken);

            var primerDocumento = documentos.First();

            var primerDocumentoPorFirmar = primerDocumento.ObtenerDocumentoVigenteParaFirma(primerDocumento.Solicitud.InCodigo);

            string rutaPrimerDocumento =
                Path.Combine(
                    primerDocumentoPorFirmar!.Ruta,
                    $"{primerDocumentoPorFirmar.UltimoNombreOriginal}.{primerDocumentoPorFirmar.ExtensionOriginal}");

            await using var streamPrimerDocumento =
                await _fileStorageService.GetFileAsync(
                    rutaPrimerDocumento,
                    cancellationToken)
                ?? throw new ValidacionAplicacion.ApplicationException(
                    ApplicationErrors.ArchivoNotFound);

            streamPrimerDocumento.Position = 0;

            var informacionPagina =
                _orientacionPaginaPdfService
                    .ObtenerPrimeraPagina(
                        streamPrimerDocumento);

            var firmaRequest =
                await _firmaPreparationService
                    .PrepararDocumentoFirmaAsync(
                        primerDocumento,
                        primerDocumento.Solicitud.CodigoSolicitud,
                        idUsuario,
                        documentos.Count,
                        informacionPagina,
                        cancellationToken);

            foreach (var item in documentos)
            {
                var documentoPorFirmar = item.ObtenerDocumentoVigenteParaFirma(item.Solicitud.InCodigo);

                var detalleFirma = item.DetallesFirma.FirstOrDefault(x => x.IdSolicitudDocumento == item.Id && x.IdFirmanteDocumento == idUsuario)
                ?? throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.DetalleFirmaInvalid);

                detalleFirma.ValidacionDocumentoFirmado();

                string rutaCompleta = Path.Combine(documentoPorFirmar!.Ruta, $"{documentoPorFirmar!.UltimoNombreOriginal}.{documentoPorFirmar!.ExtensionOriginal}");
                await using var stream = await _fileStorageService.GetFileAsync(rutaCompleta, cancellationToken);

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

                detalleFirma.Bloquear();
                detalleFirma.FechaInicioFechaFin(firmaRequest.TiempoEspera);

            }

            await _zipStreamingService.StreamZipAsync(
                   zipArchivo,
                   archivosZip,
                   cancellationToken);

            zipArchivo.Position = 0;
            firmaRequest.ArchivoStream = zipArchivo;
            var resultado = await _firmaPort.FirmarBatchAsync(firmaRequest, cancellationToken);
            resultado.TiempoEspera = firmaRequest.TiempoEspera;

            foreach (var item in documentos)
            {
                var detalleFirma = item.DetallesFirma.FirstOrDefault(x => x.IdSolicitudDocumento == item.Id && x.IdFirmanteDocumento == idUsuario)
                ?? throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.DetalleFirmaInvalid);
                detalleFirma.SetearCodigoFirma(resultado.CodigoProceso!);
            }

            return resultado;
        }
        finally
        {
            foreach (var item in archivosZip)
            {
                await item.Content.DisposeAsync();
            }
        }
    }
}
