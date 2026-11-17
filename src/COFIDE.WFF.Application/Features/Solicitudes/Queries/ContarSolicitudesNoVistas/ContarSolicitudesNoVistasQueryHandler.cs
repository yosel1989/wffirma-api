using AutoMapper;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;

namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.ContarSolicitudesNoVistas;

public sealed class ContarSolicitudesNoVistasQueryHandler(
        IMapper mapper,
        ICurrentUserService currentUser,
        ISolicitudQueryRepository solicitudQueryRepository
    ) : BaseQueryHandler<ContarSolicitudesNoVistasQuery, int>(mapper)
{
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly ISolicitudQueryRepository _solicitudQueryRepository = solicitudQueryRepository;

    public override Task<int> Handle(ContarSolicitudesNoVistasQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserIdLocal is not long idUsuario)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.UserNotAuthenticated);

        return _solicitudQueryRepository.ContarSolicitudesPorAprobarNoVistas(idUsuario, cancellationToken);
    }
}
