using AutoMapper;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;

namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerPuntosRestauracion;

public sealed class ObtenerPuntosRestauracionQueryHandler(
    IMapper mapper,
    ISolicitudRestauracionFlujoQueryRepository solicitudRestauracionFlujoQueryRepository)
    : BaseQueryHandler<ObtenerPuntosRestauracionQuery, IReadOnlyList<PuntoRestauracionDto>>(mapper)
{
    private readonly ISolicitudRestauracionFlujoQueryRepository _solicitudRestauracionFlujoQueryRepository = solicitudRestauracionFlujoQueryRepository;

    public override Task<IReadOnlyList<PuntoRestauracionDto>> Handle(
        ObtenerPuntosRestauracionQuery request,
        CancellationToken cancellationToken)
        => _solicitudRestauracionFlujoQueryRepository.ObtenerPuntosRestauracion(request.IdSolicitud, cancellationToken);
}
