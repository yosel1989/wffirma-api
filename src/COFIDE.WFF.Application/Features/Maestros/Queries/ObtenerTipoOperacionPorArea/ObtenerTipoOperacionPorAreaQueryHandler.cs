using AutoMapper;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
namespace COFIDE.WFF.Application.Features.Maestros.Queries.ObtenerTipoOperacionPorArea;

public sealed class ObtenerTipoOperacionPorAreaQueryHandler(
        IMapper mapper,
        IMaestroRepository tipoOperacionQueryRepository
    ) : BaseQueryHandler<ObtenerTipoOperacionPorAreaQuery, List<TipoOperacionComboDto>>(mapper)
{
    private readonly IMaestroRepository _tipoOperacionQueryRepository = tipoOperacionQueryRepository;

    public override async Task<List<TipoOperacionComboDto>> Handle(
        ObtenerTipoOperacionPorAreaQuery request, CancellationToken cancellationToken)
    {
        var dtoItems = await _tipoOperacionQueryRepository.GetTipoOperacionCombo(request.IdArea, cancellationToken);
        return dtoItems;
    }
}
