using AutoMapper;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Domain.DomainServices;
using COFIDE.WFF.Domain.Entities;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;
namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.PermisoSolicitud;

public sealed class PermisoSolicitudQueryHandler(
        IMapper mapper,
        ISolicitudRepository solicitudRepository,
        ICurrentUserService currentUser,
        IPlantillaRepository plantillaRepository,
        IParametroRepository parametroRepository,
        IWorkflowDomainService workflowDomainService
    )
    : BaseQueryHandler<PermisoSolicitudQuery, bool>(mapper)
{
    private readonly ISolicitudRepository _solicitudRepository = solicitudRepository;
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly IPlantillaRepository _plantillaRepository = plantillaRepository;
    private readonly IParametroRepository _parametroRepository = parametroRepository;
    private readonly IWorkflowDomainService _workflowDomainService = workflowDomainService;

    public override async Task<bool> Handle(PermisoSolicitudQuery request, CancellationToken cancellationToken)
    {
        long usuarioActual = _currentUser.UserIdLocal!.Value;
        long idArea = _currentUser.UserAreaId!.Value;
        var rolUusario = _currentUser.RolUsuario;
        var usuario = new Usuario(usuarioActual, rolUusario!.Value);

        var solicitud = await _solicitudRepository.ObtenerAgregadoSolicitudAsync(request.Id, cancellationToken, true);
        if (solicitud is null)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.SolicitudInvalid);

        if (solicitud.SolicitudPlantilla != null)
        {
            var plantilla = await _plantillaRepository.ObtenerAgregadoPlantillaAsync(solicitud.SolicitudPlantilla.IdPlantilla, cancellationToken);
            if (plantilla is null)
                throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.PlantillaInvalid);

            var tipoAprobadorDict = (await _parametroRepository
                 .ObtenerDetallePorCodigoParametro("TIPO_APROBADOR", cancellationToken))
                 .ToDictionary(
                     x => x.Id,
                     x => new ParametroDetalle(x.Id, x.Codigo, x.Valor)
                 );

            _workflowDomainService.ConstruirFirmanteFlujoEtapa(solicitud, plantilla, tipoAprobadorDict);
        }
        solicitud.ValidarPermisoSolicitud(usuarioActual, idArea, request.Revisar, usuario);

        return true;
    }
}
