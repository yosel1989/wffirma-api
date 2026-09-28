using AutoMapper;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;
namespace COFIDE.WFF.Application.Features.Maestros.Queries.ObtenerSubTipoSolicitud;

public sealed class ObtenerSubTipoSolicitudQueryHandler(
        IMapper mapper,
        ICurrentUserService currentUser,
        IMaestroRepository maestroRepository
    ) : BaseQueryHandler<ObtenerSubTipoSolicitudQuery, List<SubTipoSolicitudDto>>(mapper)
{
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly IMaestroRepository _maestroRepository = maestroRepository;

    public override async Task<List<SubTipoSolicitudDto>> Handle(ObtenerSubTipoSolicitudQuery request, CancellationToken cancellationToken)
    {
        long idArea = _currentUser.UserAreaId
        ?? throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.UsuarioAreaAsignado);

        var data = await _maestroRepository.GetByIdTipoSolicitudAndIdAreaAsync(idArea, request.IdTipoSolicitud, cancellationToken);
        var dtoItems = _mapper.Map<List<SubTipoSolicitudDto>>(data);
        return dtoItems;
    }
}
