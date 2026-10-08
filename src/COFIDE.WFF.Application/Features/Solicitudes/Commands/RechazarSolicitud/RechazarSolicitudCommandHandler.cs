using AutoMapper;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Features.Solicitudes.Services;
namespace COFIDE.WFF.Application.Features.Solicitudes.Commands.RechazarSolicitud;

public sealed class RechazarSolicitudCommandHandler(
        IMapper mapper,
        ISolicitudRevisionService usecase
    ) : BaseCommandHandler<RechazarSolicitudCommand, string?>(mapper)
{
    private readonly ISolicitudRevisionService _usecase = usecase;

    public override async Task<string?> Handle(RechazarSolicitudCommand cmd, CancellationToken cancellationToken)
    {
        return await _usecase.Rechazar(cmd, cancellationToken);
    }
}