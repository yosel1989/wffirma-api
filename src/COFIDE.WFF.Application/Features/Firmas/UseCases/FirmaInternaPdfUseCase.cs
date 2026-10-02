using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Firmas.Commands.FirmaInternaPdf;
using COFIDE.WFF.Application.Features.Firmas.DTOs;
using COFIDE.WFF.Application.Features.Firmas.Ports.In;
using COFIDE.WFF.Application.Features.Firmas.Ports.Out;
using COFIDE.WFF.Application.Features.Firmas.Services;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using Microsoft.Extensions.Logging;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;

namespace COFIDE.WFF.Application.Features.Firmas.UseCases;

public class FirmaInternaPdfUseCase(
        ICurrentUserService currentUser,
        ISolicitudReadPort solicitudRead,
        IFileStorageService fileStorageService,
        IDocumentoFirmadoValidator documentoFirmadoValidator,
        IDetalleFirmaRepository detalleFirmaRepository,
        IFirmaPort firmaPort,
        IFirmaPreparationService firmaPreparationService,
        IOrientacionPaginaPdfService orientacionPaginaPdfService,
        ILogger<FirmaInternaPdfUseCase> logger
    ) : IFirmaInternaPdfUseCase
{
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly ISolicitudReadPort _solicitudRead = solicitudRead;
    private readonly IFileStorageService _fileStorageService = fileStorageService;
    private readonly IDocumentoFirmadoValidator _documentoFirmadoValidator = documentoFirmadoValidator;
    private readonly IDetalleFirmaRepository _detalleFirmaRepository = detalleFirmaRepository;
    private readonly IFirmaPort _firmaPort = firmaPort;
    private readonly IFirmaPreparationService _firmaPreparationService = firmaPreparationService;
    private readonly IOrientacionPaginaPdfService _orientacionPaginaPdfService = orientacionPaginaPdfService;
    private readonly ILogger<FirmaInternaPdfUseCase> _logger = logger;

    public async Task<FirmaInternaResponseDto?> Firmar(
        FirmaInternaPdfCommand request,
        CancellationToken cancellationToken)
    {

        var documento =
            await _solicitudRead.ObtenerDocumentoParaFirmaInternaMasivaRead(
                request.IdSolicitud,
                request.IdSolicitudDocumento,
                cancellationToken,
                true)
            ?? throw new ValidacionAplicacion.ApplicationException(
                ApplicationErrors.DocumentosNotFound);

        long idUsuario = _currentUser.UserIdLocal!.Value;

        var documentoPorFirmar =
            documento.ObtenerDocumentoVigenteParaFirma(
                documento.Solicitud.InCodigo);

        var listaDniYaFirmados =
            documento.ObtenerDnisFirmados();

        var detalleFirma =
            documento.DetallesFirma.FirstOrDefault(x =>
                x.IdSolicitudDocumento == documento.Id &&
                x.IdFirmanteDocumento == idUsuario)
            ?? throw new ValidacionAplicacion.ApplicationException(
                ApplicationErrors.DetalleFirmaInvalid);

        detalleFirma.ValidacionDocumentoFirmado();

        string rutaCompleta =
            _fileStorageService.CombinarRuta(
                documentoPorFirmar.Ruta,
                $"{documentoPorFirmar.UltimoNombreOriginal}.{documentoPorFirmar.ExtensionOriginal}");

        await using var stream =
            await _fileStorageService.GetFileAsync(
                rutaCompleta,
                cancellationToken)
            ?? throw new ValidacionAplicacion.ApplicationException(
                ApplicationErrors.ArchivoNotFound);

        bool tieneDocumentoFirmado =
            detalleFirma.TieneDocumentoFirmado();

        if (tieneDocumentoFirmado)
        {
            var validacionArchivo =
                await _documentoFirmadoValidator
                    .ValidarFirmasAnterioresAsync(
                        stream,
                        documentoPorFirmar.Hash!,
                        listaDniYaFirmados);

            if (!validacionArchivo.EsValido)
            {
                throw new ValidacionAplicacion.ApplicationException(
                    ApplicationErrors.FirmaDocumentoInvalid.Format(
                        validacionArchivo.Mensaje!));
            }
        }

        stream.Position = 0;

        var informacionPagina =
            _orientacionPaginaPdfService
                .ObtenerPrimeraPagina(
                    stream);

        stream.Position = 0;

        var firmaRequest =
            await _firmaPreparationService
                .PrepararDocumentoFirmaAsync(
                    documento,
                    detalleFirma.CodigoReferencial!,
                    idUsuario,
                    1,
                    informacionPagina,
                    cancellationToken);

        Stream archivoParaFirmar = stream;
        archivoParaFirmar.Position = 0;

        firmaRequest.ArchivoStream =
            archivoParaFirmar;

        var resultado =
            await _firmaPort.FirmarDocumentoAsync(
                firmaRequest,
                cancellationToken);

        await ActualizarDetalleFirmaAsync(
            detalleFirma,
            firmaRequest,
            resultado);

        return resultado;
    }

    private async Task ActualizarDetalleFirmaAsync(
        DetalleFirma detalleFirma,
        FirmarDocumentoRequestDto firmaRequest,
        FirmaInternaResponseDto resultado)
    {
        resultado.TiempoEspera =
            firmaRequest.TiempoEspera;

        detalleFirma.Bloquear();

        detalleFirma.FechaInicioFechaFin(
            firmaRequest.TiempoEspera / 1000);

        detalleFirma.SetearCodigoFirma(
            resultado.CodigoProceso!);

        await _detalleFirmaRepository
            .UpdateAsync(detalleFirma);
    }
}