using AutoMapper;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Ports.Out;
using COFIDE.WFF.Application.Features.Firmas.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;
namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerSolicitud;

public sealed class ObtenerSolicitudQueryHandler(
         IMapper mapper,
         ICurrentUserService currentUser,
         IDocumentoFirmadoValidator documentoFirmadoValidator,
         IFileStorageService fileStorageService,
         ISolicitudQueryRepository solicitudQueryRepository,
         IAccesoEspecialRepository accesoEspecialRepository
    ) : BaseQueryHandler<ObtenerSolicitudQuery, SolicitudDto?>(mapper)
{
    private readonly ISolicitudQueryRepository _solicitudQueryRepository = solicitudQueryRepository;
    private readonly IDocumentoFirmadoValidator _documentoFirmadoValidator = documentoFirmadoValidator;
    private readonly IFileStorageService _fileStorageService = fileStorageService;
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly IAccesoEspecialRepository _accesoEspecialRepository = accesoEspecialRepository;

    public override async Task<SolicitudDto?> Handle(ObtenerSolicitudQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserIdLocal is null)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.UserNotAuthenticated);

        var idUsuario = _currentUser.UserIdLocal.Value;

        // Abrir el detalle: solo se consume el repository si puede ver la solicitud
        // por su rol OR por un acceso especial activo (misma regla que el listado).
        // Reusar es solo del solicitante: en reuso no se aplica el acceso especial,
        // se mantiene la validación que ya hace ObtenerSolicitudPorId.
        if (!request.EsReuso)
        {
            // Solo accesos especiales activos.
            var idsAreasAccesoEspecial = await _accesoEspecialRepository.GetAreasByUserIdAsync(idUsuario);

            var puedeVer = await _solicitudQueryRepository.PuedeVerSolicitudAsync(
                request.IdSolicitud,
                idUsuario,
                _currentUser.UserAreaId,
                _currentUser.CodigoRolUsuario,
                idsAreasAccesoEspecial,
                cancellationToken);

            if (!puedeVer)
                throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.SolicitudSinAcceso);
        }

        var solicitud = await _solicitudQueryRepository.ObtenerSolicitudPorId(request.IdSolicitud, idUsuario, request.EsReuso, cancellationToken);

        if (solicitud != null && !request.EsReuso)
        {
            foreach (var item in solicitud.Documentos)
            {
                string rutaCompleta = item.RutaArchivo!;
                await using var stream = await _fileStorageService.GetFileAsync(rutaCompleta, cancellationToken);

                item.Firmado = await _documentoFirmadoValidator.TieneFirmaAsync(stream);
            }
        }

        return solicitud;
        
    }
}