using AutoMapper;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Services;
using COFIDE.WFF.Domain.DomainServices;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;
namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.DescargarSolicitudDocumentoZip;

public class DescargarSolicitudDocumentoZipQueryHandler(
        IMapper mapper,
        ISolicitudDescargaZipService solicitudDescargaZipService,
        ISolicitudQueryRepository solicitudQueryRepository,
        IAccesoEspecialRepository accesoEspecialRepository,
        ICurrentUserService currentUser
    ) : BaseQueryHandler<DescargarSolicitudDocumentoZipQuery, DescargarSolicitudDocumentoZipDto>(mapper)
{
    private readonly ISolicitudDescargaZipService _solicitudDescargaZipService = solicitudDescargaZipService;
    private readonly ISolicitudQueryRepository _solicitudQueryRepository = solicitudQueryRepository;
    private readonly IAccesoEspecialRepository _accesoEspecialRepository = accesoEspecialRepository;
    private readonly ICurrentUserService _currentUser = currentUser;

    public override async Task<DescargarSolicitudDocumentoZipDto> Handle(DescargarSolicitudDocumentoZipQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserIdLocal is null)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.UserNotAuthenticated);

        var idUsuario = _currentUser.UserIdLocal.Value;

        var idsAreasAccesoEspecial = await _accesoEspecialRepository.GetAreasByUserIdAsync(idUsuario);
        var datos = await _solicitudQueryRepository.ObtenerParaAutorizacionAsync(
            request.Id, cancellationToken);

        ReglaConsultaSolicitud.Validar(
            datos, idUsuario, _currentUser.UserAreaId,
            _currentUser.CodigoRolUsuario, idsAreasAccesoEspecial);

        var result = await _solicitudDescargaZipService.DescargarDocumentosZipAsync(
            request.Id,
            request.Documentos,
            cancellationToken);

        return new DescargarSolicitudDocumentoZipDto
        {
            Archivo = result.Archivo,
            NombreArchivo = result.NombreZip,
            ContentType = "application/zip"
        };
    }

}