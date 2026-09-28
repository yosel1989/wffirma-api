using AutoMapper;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerEstadoSolicitudCombo;

public sealed class ObtenerEstadoSolicitudComboQueryHandler(
        IMapper mapper,
        IEstadoSolicitudQueryRepository EstadoSolicitudQueryRepository
    ) : BaseQueryHandler<ObtenerEstadoSolicitudComboQuery, List<EstadoSolicitudComboDto>>(mapper)
{
    private readonly IEstadoSolicitudQueryRepository _estadoSolicitudQueryRepository = EstadoSolicitudQueryRepository;

    public override async Task<List<EstadoSolicitudComboDto>> Handle(
    ObtenerEstadoSolicitudComboQuery request, CancellationToken cancellationToken)
    {
        var dtoItems = await _estadoSolicitudQueryRepository.GetEstadoSolicitudCombo(cancellationToken);
        return dtoItems;
    }
}

