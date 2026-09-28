using AutoMapper;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.In;
namespace COFIDE.WFF.Application.Features.Solicitudes.Commands.AprobarSolicitud;

public sealed class AprobarSolicitudCommandHandler(
        IMapper mapper,
        IRevisarSolicitudUseCase usecase
    ) : BaseCommandHandler<AprobarSolicitudCommand, string?>(mapper)
{
    private readonly IRevisarSolicitudUseCase _usecase = usecase;

    public override async Task<string?> Handle(AprobarSolicitudCommand cmd, CancellationToken cancellationToken)
    {
        return await _usecase.Aprobar(cmd,  cancellationToken);
    }
}