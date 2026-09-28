using AutoMapper;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Features.Usuarios.Ports.Out;
namespace COFIDE.WFF.Application.Features.Usuarios.Commands.CerrarSesion;

public sealed class CerrarSesionCommandHandler(
        IMapper mapper,
        IAutenticacionExternaPort authPort
    ) : BaseCommandHandler<CerrarSesionCommand, bool>(mapper)
{
    private readonly IAutenticacionExternaPort _authPort = authPort;

    public override async Task<bool> Handle(CerrarSesionCommand cmd, CancellationToken cancellationToken)
    {
        return await _authPort.CerrarSesionAsync(cancellationToken);
    }
}
