using COFIDE.WFF.Application.Common.Constants;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.RestaurarSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Domain.DomainServices;
using COFIDE.WFF.Domain.Entities;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;

namespace COFIDE.WFF.Application.Features.Solicitudes.Services;

public class SolicitudRestauracionFlujoService(
    ICurrentUserService currentUser,
    ISolicitudRepository solicitudRepository,
    IPlantillaRepository plantillaRepository,
    IWorkflowDomainService workflowDomainService,
    IParametroRepository parametroRepository,
    IGeneradorCorrelativoIdRepository generadorCorrelativoIdRepository,
    IFileStorageService fileStorageService) : ISolicitudRestauracionFlujoService
{
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly ISolicitudRepository _solicitudRepository = solicitudRepository;
    private readonly IPlantillaRepository _plantillaRepository = plantillaRepository;
    private readonly IWorkflowDomainService _workflowDomainService = workflowDomainService;
    private readonly IParametroRepository _parametroRepository = parametroRepository;
    private readonly IGeneradorCorrelativoIdRepository _generadorCorrelativoIdRepository = generadorCorrelativoIdRepository;
    private readonly IFileStorageService _fileStorageService = fileStorageService;

    public async Task<string?> Restaurar(RestaurarSolicitudCommand cmd, CancellationToken cancellationToken)
    {
        if (_currentUser.UserIdLocal is null)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.UserNotAuthenticated);

        var solicitud = await _solicitudRepository.ObtenerSolicitudParaRestauracion(cmd.Id, cancellationToken, true)
            ?? throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.SolicitudInvalid);

        if (solicitud.SolicitudPlantilla is null)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.PlantillaInvalid);

        var plantilla = await _plantillaRepository.ObtenerAgregadoPlantillaAsync(solicitud.SolicitudPlantilla.IdPlantilla, cancellationToken)
            ?? throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.PlantillaInvalid);

        var tipoAprobadorDict = (await _parametroRepository
                .ObtenerDetallePorCodigoParametro(CodigosParametro.TipoAprobador, cancellationToken))
            .ToDictionary(
                x => x.Id,
                x => new ParametroDetalle(x.Id, x.Codigo, x.Valor));

        _workflowDomainService.ConstruirFirmanteFlujoEtapa(solicitud, plantilla, tipoAprobadorDict);

        var cantidadDetalles = solicitud.SolicitudPlantilla.FirmantesFlujo.Count(f => f.Estado)
            * solicitud.Documentos.Count(d => d.Estado);
        var rangoDetalles = await _generadorCorrelativoIdRepository
            .ObtenerBloqueSiguienteIdDetalleFirmaAsync(cantidadDetalles, cancellationToken);
        var indiceDetalle = 0;
        Func<long> generarIdDetalleFirma = () =>
        {
            if (indiceDetalle >= rangoDetalles.Count)
                throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.DetalleFirmaInvalid);

            return rangoDetalles[indiceDetalle++];
        };

        var rutas = solicitud.RestaurarFlujo(cmd.IdPuntoRestauracion, _currentUser.UserIdLocal.Value, generarIdDetalleFirma);

        foreach (var ruta in rutas)
            await _fileStorageService.DeleteFileAsync(ruta, cancellationToken);

        _solicitudRepository.GuardarFlujoSolicitudAsync(solicitud);
        return solicitud.CodigoSolicitud;
    }
}
