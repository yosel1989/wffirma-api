using AutoMapper;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
namespace COFIDE.WFF.Application.Features.Maestros.Queries.ObtenerTipoSolicitud;

public sealed class ObtenerTipoSolicitudQueryHandler(
        IMapper mapper,
        ICurrentUserService currentUser,
        IMaestroRepository maestroRepository
    ) : BaseQueryHandler<ObtenerTipoSolicitudQuery, List<TipoSolicitudDto>>(mapper)
{
    private readonly ICurrentUserService _currentUser= currentUser;
    private readonly IMaestroRepository _maestroRepository = maestroRepository;

    public override async Task<List<TipoSolicitudDto>> Handle(ObtenerTipoSolicitudQuery request, CancellationToken cancellationToken)
    {
        long idArea = _currentUser.UserAreaId!.Value;
        var data = await _maestroRepository.GetTipoSolicitudByIdAreaAsync(idArea, cancellationToken);
        var dtoItems = _mapper.Map<List<TipoSolicitudDto>>(data);
        return dtoItems;
    }
}
