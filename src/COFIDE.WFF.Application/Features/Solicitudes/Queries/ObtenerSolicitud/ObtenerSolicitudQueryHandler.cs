using AutoMapper;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Firmas.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Services;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;
namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerSolicitud;

public sealed class ObtenerSolicitudQueryHandler(
         IMapper mapper,
         ICurrentUserService currentUser,
         IDocumentoFirmadoValidator documentoFirmadoValidator,
         IFileStorageService fileStorageService,
         ISolicitudQueryRepository solicitudQueryRepository,
         ISolicitudConsultaAuthorizationService solicitudConsultaAuthorizationService
    ) : BaseQueryHandler<ObtenerSolicitudQuery, SolicitudDto?>(mapper)
{
    private readonly ISolicitudQueryRepository _solicitudQueryRepository = solicitudQueryRepository;
    private readonly IDocumentoFirmadoValidator _documentoFirmadoValidator = documentoFirmadoValidator;
    private readonly IFileStorageService _fileStorageService = fileStorageService;
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly ISolicitudConsultaAuthorizationService _solicitudConsultaAuthorizationService = solicitudConsultaAuthorizationService;

    public override async Task<SolicitudDto?> Handle(ObtenerSolicitudQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserIdLocal is null)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.UserNotAuthenticated);

        var idUsuario = _currentUser.UserIdLocal.Value;

        if (request.EsReuso)
            await _solicitudConsultaAuthorizationService.AutorizarReusoPorIdAsync(
                request.IdSolicitud, cancellationToken);
        else
            await _solicitudConsultaAuthorizationService.AutorizarPorIdAsync(
                request.IdSolicitud, cancellationToken);

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