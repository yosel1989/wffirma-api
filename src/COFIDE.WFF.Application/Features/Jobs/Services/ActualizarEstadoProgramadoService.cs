using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;

namespace COFIDE.WFF.Application.Features.Jobs.Services;

public class ActualizarEstadoProgramadoService(
        IDetalleFirmaRepository repository
    ) : IActualizarEstadoProgramadoService
{
    private readonly IDetalleFirmaRepository _repository = repository;

    public async Task<int> EjecutarAsync(CancellationToken cancellationToken)
    {
        return await _repository.ActualizarEstadosSolicitudProgramadoAsync(cancellationToken);
    }
}
