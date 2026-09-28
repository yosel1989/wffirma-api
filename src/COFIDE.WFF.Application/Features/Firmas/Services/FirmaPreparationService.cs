using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Features.Firmas.DTOs;
using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.DomainServices;
using COFIDE.WFF.Domain.Entities;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;
using ValueObject = COFIDE.WFF.Domain.ValueObjects;
namespace COFIDE.WFF.Application.Features.Firmas.Services;

public class FirmaPreparationService(
        IWorkflowDomainService workflowDomainService,
        IPlantillaRepository plantillaRepository,
        IParametroRepository parametroRepository
    ) : IFirmaPreparationService
{
    private readonly IWorkflowDomainService _workflowDomainService = workflowDomainService;
    private readonly IPlantillaRepository _plantillaRepository = plantillaRepository;
    private readonly IParametroRepository _parametroRepository = parametroRepository;

    public async Task<FirmarDocumentoRequestDto> PrepararDocumentoFirmaAsync(
       SolicitudDocumento solicitudDocumento,
       string codigoReferencial,
       long idUsuario,
       int cantidadDocumentos,
       InformacionPaginaPdfDto? informacionPagina,
       CancellationToken cancellationToken)
    {
        bool esMasivo = cantidadDocumentos > 1;

        SolicitudPlantilla solicitudPlantilla = solicitudDocumento.Solicitud.SolicitudPlantilla;

        var plantilla = await _plantillaRepository.ObtenerAgregadoPlantillaAsync(solicitudPlantilla.IdPlantilla, cancellationToken, true);

        var tipoAprobadorDict =
            (await _parametroRepository
                .ObtenerDetallePorCodigoParametro(
                    "TIPO_APROBADOR",
                    cancellationToken))
            .ToDictionary(
                x => x.Id,
                x => new ParametroDetalle(
                    x.Id,
                    x.Codigo,
                    x.Valor));

        var tiempoProcesoFirma = await _parametroRepository.ObtenerTiempoProcesoFirmadoArchivoAsync(cancellationToken);
        var tamanioFirma = await _parametroRepository.ObtenerTamanioFirmaAsync(cancellationToken);
        var posicionFirma = await _parametroRepository.ObtenerPosicionFirmaAsync(cancellationToken);
        var configFirma = await _parametroRepository.ConfiguracionFirmaAsync(cancellationToken);

        _workflowDomainService.AsignarUsuarioAsignado(solicitudPlantilla, plantilla!, tipoAprobadorDict);

        var usuario = solicitudPlantilla.FirmantesFlujo.FirstOrDefault(x => x.IdUsuarioAsignado == idUsuario);

        if (usuario is null)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.DetalleFirmaInvalid);

        var configuracionFirma = ObtenerConfiguracionFirma(
            solicitudDocumento,
            usuario,
            configFirma.NombreTag,
            esMasivo,
            tamanioFirma.FirmaAnchoVisador,
            tamanioFirma.FirmaAltoVisador,
            posicionFirma,
            informacionPagina);

        decimal ancho = usuario.EsFirmante
          ? tamanioFirma.FirmaAnchoFirmante
          : tamanioFirma.FirmaAnchoVisador;

        decimal alto = usuario.EsFirmante
            ? tamanioFirma.FirmaAltoFirmante
            : tamanioFirma.FirmaAltoVisador;

        var firmarDocumentoRequestDto = new FirmarDocumentoRequestDto
        {
            CodigoArchivo = codigoReferencial,
            VisSigText = ObtenerTextoFirma(usuario),
            Tags = configuracionFirma.Tag,
            TipoFirma = configuracionFirma.TipoFirma,
            VisSigPage = configuracionFirma.Pagina,
            VisSigX = configuracionFirma.X,
            VisSigY = configuracionFirma.Y,
            VisSigWidth = ancho,
            VisSigHeight = alto,
            TiempoEspera =
              ((cantidadDocumentos * tiempoProcesoFirma.TiempoArchivo)
              + tiempoProcesoFirma.TiempoMargenSeguridad) * 1000,
            EsFirmante = usuario.EsFirmante,
            EsVisador = usuario.EsVisador,
            VisSigTextSize = usuario.EsFirmante ? configFirma.TamanioLetraFirmante : configFirma.TamanioLetraVisado
        };

        return firmarDocumentoRequestDto;
    }

    private static ConfiguracionFirma ObtenerConfiguracionFirma(
    SolicitudDocumento solicitudDocumento,
    SolicitudFirmanteFlujo usuario,
    string? tag,
    bool esMasivo,
    decimal anchoFirmaVisador,
    decimal altoFirmaVisador,
    PosicionFirmaDto posicionFirma,
    InformacionPaginaPdfDto? informacionPagina)
    {
        var esHorizontal =
            informacionPagina?.EsHorizontal == true;

        var posiciones =
            solicitudDocumento.ObtenerPosicionFirmaVisador(
                posicionFirma.PosicionXVisador,
                posicionFirma.PosicionYVisador,
                anchoFirmaVisador,
                altoFirmaVisador,
                esHorizontal,
                informacionPagina?.Ancho,
                informacionPagina?.Alto);

        if (esMasivo)
        {
            return usuario.EsFirmante
                ? new ConfiguracionFirma(
                    ValueObject.TipoFirma.FirmaConTag.Codigo,
                    null,
                    null,
                    null,
                    tag)
                : new ConfiguracionFirma(
                    ValueObject.TipoFirma.FirmaConParametros.Codigo,
                    1,
                    posiciones.X,
                    posiciones.Y,
                    null);
        }

        if (usuario.EsFirmante)
        {
            int tipoFirma = solicitudDocumento.DocumentoConTag()
                ? ValueObject.TipoFirma.FirmaConTag.Codigo
                : ValueObject.TipoFirma.FirmaEsquinaSuperiorIzquierda.Codigo;

            return new ConfiguracionFirma(
                tipoFirma,
                null,
                null,
                null,
                tag);
        }

        return new ConfiguracionFirma(
            ValueObject.TipoFirma.FirmaConParametros.Codigo,
            1,
            posiciones.X,
            posiciones.Y,
            null);
    }

    private static string ObtenerTextoFirma(SolicitudFirmanteFlujo usuario)
    {
        if (usuario.EsFirmante)
        {
            return "Firmado digitalmente por: <SIGNER><br>Motivo: Soy el autor del documento<br>Fecha:<DATE>";
        }

        if (usuario.EsVisador)
        {
            return "Firmado digitalmente por: <SIGNER><br>Motivo: Doy Vo Bo<br>Fecha:<DATE>";
        }

        return string.Empty;
    }

    private sealed record ConfiguracionFirma(
      int TipoFirma,
      int? Pagina,
      decimal? X,
      decimal? Y,
      string? Tag);
}
