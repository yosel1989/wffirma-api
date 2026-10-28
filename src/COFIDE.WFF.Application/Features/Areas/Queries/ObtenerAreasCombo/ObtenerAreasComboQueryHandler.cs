using AutoMapper;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Features.Areas.Ports.Out;
namespace COFIDE.WFF.Application.Features.Areas.Queries.ObtenerAreasCombo;

public sealed class ObtenerAreasComboQueryHandler(
        IMapper mapper,
        IAreaRepository areaRepository
    ) : BaseQueryHandler<ObtenerAreasComboQuery, List<AreasComboDto>>(mapper)
{
    private readonly IAreaRepository _areaRepository = areaRepository;

    public override async Task<List<AreasComboDto>> Handle(
        ObtenerAreasComboQuery request, CancellationToken cancellationToken)
    {
        var dtoItems = await _areaRepository.GetAreaCombo(request.Tipo, cancellationToken);
        return dtoItems;
    }
}