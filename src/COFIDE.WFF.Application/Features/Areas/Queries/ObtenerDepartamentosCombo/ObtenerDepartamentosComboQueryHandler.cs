using AutoMapper;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Features.Areas.Ports.Out;
namespace COFIDE.WFF.Application.Features.Areas.Queries.ObtenerDepartamentosCombo;

public sealed class ObtenerDepartamentosComboQueryHandler(
        IMapper mapper,
        IAreaRepository areaRepository
    ) : BaseQueryHandler<ObtenerDepartamentosComboQuery, List<DepartamentosComboDto>>(mapper)
{
    private readonly IAreaRepository _areaRepository = areaRepository;

    public override async Task<List<DepartamentosComboDto>> Handle(
        ObtenerDepartamentosComboQuery request, CancellationToken cancellationToken)
    {
        return await _areaRepository.GetDepartamentosCombo(request.CodigoGerencia, cancellationToken);
    }
}