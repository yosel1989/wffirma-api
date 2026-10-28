using AutoMapper;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Features.Solicitudes.Services;

namespace COFIDE.WFF.Application.Features.Solicitudes.Commands.RestaurarSolicitud;

public sealed class RestaurarSolicitudCommandHandler(
    IMapper mapper,
    ISolicitudRestauracionFlujoService usecase)
    : BaseCommandHandler<RestaurarSolicitudCommand, string?>(mapper)
{
    private readonly ISolicitudRestauracionFlujoService _usecase = usecase;

    public override Task<string?> Handle(RestaurarSolicitudCommand cmd, CancellationToken cancellationToken)
        => _usecase.Restaurar(cmd, cancellationToken);
}
