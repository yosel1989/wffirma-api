using AutoMapper;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
namespace COFIDE.WFF.Application.Features.Maestros.Queries.ObtenerTipoSolicitudPorArea;

public sealed class ObtenerTipoSolicitudPorAreaQueryHandler(
        IMapper mapper,
        IMaestroRepository maestroRepository
    ) : BaseQueryHandler<ObtenerTipoSolicitudPorAreaQuery, List<TipoSolicitudComboDto>>(mapper)
{
    private readonly IMaestroRepository _maestroRepository = maestroRepository;

    public override async Task<List<TipoSolicitudComboDto>> Handle(
        ObtenerTipoSolicitudPorAreaQuery request, CancellationToken cancellationToken)
    {
        var dtoItems = await _maestroRepository.GetTipoSolicitudCombo(request.IdArea, cancellationToken);
        return dtoItems;
    }
}