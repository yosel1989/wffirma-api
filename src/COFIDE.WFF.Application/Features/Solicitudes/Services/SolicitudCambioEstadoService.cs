using COFIDE.WFF.Application.Common.Constants;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.CambiarEstadoSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.DomainServices;
using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Domain.ValueObjects;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;
using EstadoSolicitud = COFIDE.WFF.Domain.ValueObjects.EstadoSolicitud;

namespace COFIDE.WFF.Application.Features.Solicitudes.Services;

public class SolicitudCambioEstadoService(
    ICurrentUserService currentUser,
    ISolicitudRepository solicitudRepository,
    ISolicitudQueryRepository solicitudQueryRepository,
    IPlantillaRepository plantillaRepository,
    IWorkflowDomainService workflowDomainService,
    IParametroRepository parametroRepository,
    IGeneradorCorrelativoIdRepository generadorCorrelativoIdRepository) : ISolicitudCambioEstadoService
{
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly ISolicitudRepository _solicitudRepository = solicitudRepository;
    private readonly ISolicitudQueryRepository _solicitudQueryRepository = solicitudQueryRepository;
    private readonly IPlantillaRepository _plantillaRepository = plantillaRepository;
    private readonly IWorkflowDomainService _workflowDomainService = workflowDomainService;
    private readonly IParametroRepository _parametroRepository = parametroRepository;
    private readonly IGeneradorCorrelativoIdRepository _generadorCorrelativoIdRepository = generadorCorrelativoIdRepository;

    public async Task<string?> CambiarEstado(CambiarEstadoSolicitudCommand cmd, CancellationToken cancellationToken)
    {
        if (_currentUser.UserIdLocal is null)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.UserNotAuthenticated);

        var idUsuario = _currentUser.UserIdLocal.Value;
        var destino = EstadoSolicitud.FromCodigo(cmd.IdNuevoEstado);

        var solicitud = await _solicitudRepository.ObtenerSolicitudParaRestauracion(cmd.Id, cancellationToken, true)
            ?? throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.SolicitudInvalid);

        solicitud.ValidarCambioEstado(destino, cmd.Motivo);
        var codigoSolicitud = solicitud.CodigoSolicitud;

        if (solicitud.SolicitudPlantilla is null)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.PlantillaInvalid);

        await PrepararFlujo(solicitud, cancellationToken);

        if (solicitud.EstadoSolicitud == EstadoSolicitud.Aprobado)
        {
            solicitud.ReabrirDesdeAprobada(idUsuario, cmd.Motivo);
        }
        else if (solicitud.EstadoSolicitud == EstadoSolicitud.Rechazado)
        {
            var generarId = await CrearGeneradorDetalleFirma(solicitud, cancellationToken);
            solicitud.ReabrirDesdeRechazada(idUsuario, cmd.Motivo, generarId);
        }
        else
        {
            await DevolverAlCreador(solicitud, destino, idUsuario, cmd.Motivo, cancellationToken);
        }

        _solicitudRepository.GuardarFlujoSolicitudAsync(solicitud);
        return codigoSolicitud;
    }

    private async Task DevolverAlCreador(
        Solicitud solicitud,
        EstadoSolicitud destino,
        long idUsuario,
        string motivo,
        CancellationToken cancellationToken)
    {
        long idHistorialRevision = 0;
        int ciclo = 0;

        if (destino == EstadoSolicitud.Rechazado)
        {
            idHistorialRevision = await _generadorCorrelativoIdRepository.ObtenerSiguienteIdHistorialRevisionAsync(cancellationToken);
            ciclo = await _solicitudRepository.ObtenerSiguienteCicloAsync(solicitud.SolicitudPlantilla.Id, cancellationToken);
        }

        var nombreUsuario = $"{_currentUser.NombreUsuario} {_currentUser.ApellidoPaterno} {_currentUser.ApellidoMaterno}".Trim();

        string? correoCreador = null;
        if (destino == EstadoSolicitud.Borrador)
        {
            correoCreador = (await _solicitudQueryRepository.ObtenerParaNotificaciones(solicitud.Id, cancellationToken))?.correoSolicitante;
        }

        solicitud.DevolverAlCreador(destino, idUsuario, nombreUsuario, motivo, idHistorialRevision, ciclo, correoCreador);
    }

    private async Task PrepararFlujo(Solicitud solicitud, CancellationToken cancellationToken)
    {
        var plantilla = await _plantillaRepository.ObtenerAgregadoPlantillaAsync(solicitud.SolicitudPlantilla.IdPlantilla, cancellationToken)
            ?? throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.PlantillaInvalid);

        var tipoAprobadorDict = (await _parametroRepository
                .ObtenerDetallePorCodigoParametro(CodigosParametro.TipoAprobador, cancellationToken))
            .ToDictionary(
                x => x.Id,
                x => new ParametroDetalle(x.Id, x.Codigo, x.Valor));

        _workflowDomainService.ConstruirFirmanteFlujoEtapa(solicitud, plantilla, tipoAprobadorDict);
    }

    private async Task<Func<long>> CrearGeneradorDetalleFirma(Solicitud solicitud, CancellationToken cancellationToken)
    {
        var cantidad = solicitud.SolicitudPlantilla.FirmantesFlujo.Count(f => f.Estado)
            * solicitud.Documentos.Count(d => d.Estado);

        var rango = await _generadorCorrelativoIdRepository
            .ObtenerBloqueSiguienteIdDetalleFirmaAsync(cantidad, cancellationToken);

        var indice = 0;
        return () =>
        {
            if (indice >= rango.Count)
                throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.DetalleFirmaInvalid);

            return rango[indice++];
        };
    }
}
