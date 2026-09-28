using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
using MediatR;
namespace COFIDE.WFF.Application.Features.Jobs.ActualizarEstadoProgramado;

public class ActualizarEstadosProgramadoCommandHandler(
        IParametroRepository repository
    ) : IRequestHandler<ActualizarEstadosProgramadoCommand, int>
{
    private readonly IParametroRepository _repository = repository;

    public async Task<int> Handle(
        ActualizarEstadosProgramadoCommand request,
        CancellationToken cancellationToken)
    {
        await _repository.ObtenerParametroDetallePorIdAsync(request.Id, cancellationToken);
        return 1;
    }
}