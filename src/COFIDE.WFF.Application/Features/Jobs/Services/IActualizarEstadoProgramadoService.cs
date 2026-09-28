namespace COFIDE.WFF.Application.Features.Jobs.Services;
public interface IActualizarEstadoProgramadoService
{
    Task<int> EjecutarAsync(CancellationToken cancellationToken);
}
