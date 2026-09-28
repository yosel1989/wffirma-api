using AutoMapper;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;
namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerSolicitudPanelControl;

public sealed class ObtenerSolicitudPanelControlQueryHandler(
        IMapper mapper,
        ICurrentUserService currentUser,
        ISolicitudQueryRepository solicitudQueryRepository
    ) : BaseQueryHandler<ObtenerSolicitudPanelControlQuery, SolicitudPanelControlDto?>(mapper)
{
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly ISolicitudQueryRepository _solicitudQueryRepository = solicitudQueryRepository;

    public override async Task<SolicitudPanelControlDto?> Handle(ObtenerSolicitudPanelControlQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserIdLocal is null)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.UserNotAuthenticated);

        var idUsuario = _currentUser.UserIdLocal;
        return await _solicitudQueryRepository.ObtenerSolicitudPanelControl(idUsuario, cancellationToken);
    }
}

