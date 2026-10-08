using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.DTOs;
using COFIDE.WFF.Application.Features.Solicitudes.Mappings;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Services;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Factories;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Inputs;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;
namespace COFIDE.WFF.Application.Features.Solicitudes.Services;

public class SolicitudReusoService(
    ICurrentUserService currentUser,
    ISolicitudRepository solicitudRepository,
    ISolicitudCreacionService solicitudCreacionService,
    ISolicitudDocumentoService solicitudDocumentoService,
    ISolicitudSustentoService solicitudSustentoService,
    ISolicitudPlantillaService solicitudPlantillaService,
    ISolicitudDocumentoTagService solicitudDocumentoTagService,
    IGeneradorCorrelativoIdRepository generadorCorrelativoIdRepository
) : ISolicitudReusoService
{
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly ISolicitudRepository _solicitudRepository = solicitudRepository;
    private readonly ISolicitudCreacionService _solicitudCreacionService = solicitudCreacionService;
    private readonly ISolicitudDocumentoService _solicitudDocumentoService = solicitudDocumentoService;
    private readonly ISolicitudSustentoService _solicitudSustentoService = solicitudSustentoService;
    private readonly ISolicitudPlantillaService _solicitudPlantillaService = solicitudPlantillaService;
    private readonly ISolicitudDocumentoTagService _solicitudDocumentoTagService = solicitudDocumentoTagService;
    private readonly IGeneradorCorrelativoIdRepository _generadorCorrelativoIdRepository = generadorCorrelativoIdRepository;

    public async Task<(Solicitud, Plantilla?)> Reusar(SolicitudRequestDto cmd, long idSolicitudOrigen, bool esBorrador, CancellationToken cancellationToken)
    {
        long idUSuario = _currentUser.UserIdLocal!.Value;
        long idArea = _currentUser.UserAreaId!.Value;
        long idAreaPadre = _currentUser.UserAreaPadreId!.Value;
        string codigoArea = _currentUser.UserAreaDescripcionCorta!;
        string gerenciaUsuario = _currentUser.UserGerenciaNombre!;

        var solicitudOrigen = await _solicitudRepository.ObtenerAgregadoSolicitudAsync(idSolicitudOrigen, cancellationToken)
        ?? throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.SolicitudOrigenNotFound);

        #region 01. Generacion de Correlativos
        var cantidadNuevosCodigoReferencia = cmd.CodigosReferencia?.Count ?? 0;
        var cantidadNuevosCorreos = cmd.Correos?.Count ?? 0;
        var cantidadNuevosDocumentos = cmd.Documentos?.Count ?? 0;
        var cantidadNuevosSustentos = cmd.Sustentos?.Count ?? 0;
        var rangoCodigosReferencia = await _generadorCorrelativoIdRepository.ObtenerBloqueSiguienteIdCodigosReferenciaAsync(cantidadNuevosCodigoReferencia, cancellationToken);
        var rangoCorreos = await _generadorCorrelativoIdRepository.ObtenerBloqueSiguienteIdCorreosAsync(cantidadNuevosCorreos, cancellationToken);
        var rangoDocumentos = await _generadorCorrelativoIdRepository.ObtenerBloqueSiguienteIdDocumentoAsync(cantidadNuevosDocumentos, cancellationToken);
        var rangoSustentos = await _generadorCorrelativoIdRepository.ObtenerBloqueSiguienteIdSustentoAsync(cantidadNuevosSustentos, cancellationToken);
        var indexCodigoReferencia = 0; var indexCorreo = 0; var indexDocumento = 0; var indexSustento = 0;
        Func<long> generarIdCodigoReferencia = () => rangoCodigosReferencia[indexCodigoReferencia++];
        Func<long> generarIdCorreo = () => rangoCorreos[indexCorreo++];
        Func<long> generarIdDocumento = () => rangoDocumentos[indexDocumento++];
        Func<long> generarIdSustento = () => rangoSustentos[indexSustento++];
        #endregion

        #region 02. Persistencia de Solicitud
        var solicitudBase = await _solicitudCreacionService.CrearSolicitudBase(cmd, codigoArea, idUSuario, idArea, idAreaPadre, cancellationToken);
        var nuevosDocumentos = await _solicitudDocumentoService.CrearDocumentoInput(cmd.Documentos, solicitudBase.CodigoSolicitud, solicitudBase.InCodigo, cmd.Tags, cancellationToken);
        var documentos = nuevosDocumentos;
        var nuevosSustentos = await _solicitudSustentoService.CrearDocumentoSustentoInput(cmd.Sustentos, solicitudBase.CodigoSolicitud, cancellationToken);
        var sustentos = nuevosSustentos;

        var solicitudInput = SolicitudMapping.ConstruirSolicitudInput(cmd, documentos, sustentos);
        var solicitud = SolicitudFactory.CrearDesdeRequest(solicitudInput, solicitudBase, generarIdCodigoReferencia, generarIdCorreo, generarIdDocumento, generarIdSustento);

        if (esBorrador)
            solicitud.NotificarCorreoBorrador(_currentUser.UserCorreo);
        else
            solicitud.Finalizar(cmd.Asunto!, cmd.Mensaje, cmd.EsUrgente, cmd.InCodigo, cmd.FechaLimite, cmd.PesoTotalDocu, _currentUser.UserCorreo);
        #endregion

        #region 03. Persistencia de Plantilla
        var plantilla = await _solicitudPlantillaService.CrearSolicitudPlantilla(0, cmd.Configuracion, cmd.IdPlantilla, idUSuario, gerenciaUsuario, cancellationToken);
        #endregion

        #region 04. Persistencia de Solicitud Plantilla / Firmantes / DetalleFirma
        await _solicitudPlantillaService.CrearFlujoDesdeSolicitud(plantilla, solicitud, cancellationToken);
        #endregion

        #region 05. Persistencia de Documentos TAG
        await _solicitudDocumentoTagService.CrearDocumentoTagDesdeSolicitud(solicitud, cmd.Tags, cancellationToken);
        #endregion

        return (solicitud, plantilla);
    }
}
