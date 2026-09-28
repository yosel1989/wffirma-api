using AutoMapper;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
namespace COFIDE.WFF.Application.Features.Maestros.Queries.ObtenerSubTipoSolicitudPorArea;

public sealed class ObtenerSubTipoSolicitudPorAreaQueryHandler(
        IMapper mapper,
        IMaestroRepository maestroRepository
    ) : BaseQueryHandler<ObtenerSubTipoSolicitudPorAreaQuery, List<SubTipoSolicitudComboDto>>(mapper)
{
    private readonly IMaestroRepository _maestroRepository = maestroRepository;

    public override async Task<List<SubTipoSolicitudComboDto>> Handle(
        ObtenerSubTipoSolicitudPorAreaQuery request, CancellationToken cancellationToken)
    {
        var dtoItems = await _maestroRepository.GetSubTipoSolicitudCombo(request.IdArea, request.IdTipoSolicitud, cancellationToken);
        return dtoItems;
    }
}