using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.DTOs;
using COFIDE.WFF.Application.Features.Solicitudes.Mappings;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Services;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Entities;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;
namespace COFIDE.WFF.Application.Features.Solicitudes.Services;

public class SolicitudEdicionService(
        ICurrentUserService currentUser,
        ISolicitudRepository solicitudRepository,
        ISolicitudPlantillaService solicitudPlantillaService,
        ISolicitudDocumentoService solicitudDocumentoService,
        ISolicitudSustentoService solicitudSustentoService,
        ISolicitudDocumentoTagService solicitudDocumentoTagService,
        IGeneradorCorrelativoIdRepository generadorCorrelativoIdRepository,
        IAccesoEspecialRepository accesoEspecialRepository
    ) : ISolicitudEdicionService
{
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly ISolicitudRepository _solicitudRepository = solicitudRepository;
    private readonly ISolicitudPlantillaService _solicitudPlantillaService = solicitudPlantillaService;
    private readonly ISolicitudDocumentoService _solicitudDocumentoService = solicitudDocumentoService;
    private readonly ISolicitudSustentoService _solicitudSustentoService = solicitudSustentoService;
    private readonly ISolicitudDocumentoTagService _solicitudDocumentoTagService = solicitudDocumentoTagService;
    private readonly IGeneradorCorrelativoIdRepository _generadorCorrelativoIdRepository = generadorCorrelativoIdRepository;
    private readonly IAccesoEspecialRepository _accesoEspecialRepository = accesoEspecialRepository;

    public async Task<(Solicitud solicitud, Plantilla? plantilla)> Editar(SolicitudRequestDto cmd, long idSolicitud, bool esBorrador, CancellationToken cancellationToken)
    {
        long idUSuario = _currentUser.UserIdLocal!.Value;
        string gerenciaUsuario = _currentUser.UserGerenciaNombre!;
        var rolUusario = _currentUser.RolUsuario;
        var usuario = new Usuario(idUSuario, rolUusario!.Value);
        string correoUsuario = _currentUser.UserCorreo!;

        var solicitud = await _solicitudRepository.ObtenerAgregadoSolicitudAsync(idSolicitud, cancellationToken, tracking: true);
        if (solicitud is null)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.SolicitudInvalid);

        var areasAccesoEspecial = await _accesoEspecialRepository.GetAreasByUserIdAsync(idUSuario);
        solicitud.ValidarPermisoEscritura(usuario, _currentUser.UserAreaId, areasAccesoEspecial);

        #region 01. Generacion de Correlativos
        var cantidadNuevosDocumentos = cmd.Documentos?.Count ?? 0;
        var cantidadNuevosSustentos = cmd.Sustentos?.Count ?? 0;
        var cantidadNuevosCodigoReferencia = solicitud.ContarNuevosCodigosReferencia(cmd.CodigosReferencia);
        var cantidadNuevosCorreos = solicitud.ContarNuevosCorreos(cmd.Correos);
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
        long? idPlantillaOrigen = solicitud.SolicitudPlantilla == null ? 0 : solicitud.SolicitudPlantilla.IdPlantilla;
        if (solicitud.InCodigo != cmd.InCodigo)
            await _solicitudDocumentoService.SincronizarCambioCodigoAsync(solicitud, cmd.InCodigo, cancellationToken);
        var documentos = await _solicitudDocumentoService.CrearDocumentoInput(cmd.Documentos, solicitud.CodigoSolicitud, solicitud.InCodigo, cmd.Tags, cancellationToken);
        var sustentos = await _solicitudSustentoService.CrearDocumentoSustentoInput(cmd.Sustentos, solicitud.CodigoSolicitud, cancellationToken);
        var editSolicitud = SolicitudMapping.MapearEditarSolicitudModel(cmd, documentos, sustentos);

        if (esBorrador)
        {
            solicitud.EditarSolicitudBorrador(editSolicitud, generarIdDocumento, generarIdSustento, generarIdCodigoReferencia, generarIdCorreo,
                usuario, _currentUser.UserAreaId, areasAccesoEspecial);
            solicitud.NotificarCorreoBorrador(_currentUser.UserCorreo);
        }
        else if (solicitud.EsBorradorDevueltoPorCambioEstado())
        {
            solicitud.EditarBorradorDevueltoYEnviar(editSolicitud, generarIdDocumento, generarIdSustento, generarIdCodigoReferencia, generarIdCorreo,
                usuario, _currentUser.UserAreaId, areasAccesoEspecial, correoUsuario);
        }
        else
        {
            solicitud.EditarSolicitudBorradorYEnviar(editSolicitud, generarIdDocumento, generarIdSustento, generarIdCodigoReferencia, generarIdCorreo,
                usuario, _currentUser.UserAreaId, areasAccesoEspecial, correoUsuario);
        }
        #endregion

        #region 03. Persistencia de Plantilla
        var plantilla = await _solicitudPlantillaService.CrearSolicitudPlantilla(idPlantillaOrigen, cmd.Configuracion, cmd.IdPlantilla, idUSuario, gerenciaUsuario, cancellationToken);
        #endregion

        #region 04. Persistencia de Solicitud Plantilla / Firmantes / DetalleFirma
        await _solicitudPlantillaService.ActualizarFlujoDesdeSolicitud(plantilla, solicitud, cancellationToken);
        #endregion

        #region 05. Persistencia de Documentos TAG
        await _solicitudDocumentoTagService.AntualizarDocumentoTagDesdeSolicitud(solicitud, cmd.Tags, cancellationToken);
        await _solicitudDocumentoService.SincronizarTagDocumentoAsync(solicitud, cancellationToken);
        #endregion

        return (solicitud, plantilla);
    }

}
