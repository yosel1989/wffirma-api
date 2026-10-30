using AutoMapper;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Features.Solicitudes.Services;

namespace COFIDE.WFF.Application.Features.Solicitudes.Commands.CambiarEstadoSolicitud;

public sealed class CambiarEstadoSolicitudCommandHandler(
    IMapper mapper,
    ISolicitudCambioEstadoService usecase)
    : BaseCommandHandler<CambiarEstadoSolicitudCommand, string?>(mapper)
{
    private readonly ISolicitudCambioEstadoService _usecase = usecase;

    public override Task<string?> Handle(CambiarEstadoSolicitudCommand cmd, CancellationToken cancellationToken)
        => _usecase.CambiarEstado(cmd, cancellationToken);
}
