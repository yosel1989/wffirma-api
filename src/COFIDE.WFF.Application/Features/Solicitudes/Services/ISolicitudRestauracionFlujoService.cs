using COFIDE.WFF.Application.Features.Solicitudes.Commands.RestaurarSolicitud;

namespace COFIDE.WFF.Application.Features.Solicitudes.Services;

public interface ISolicitudRestauracionFlujoService
{
    Task<string?> Restaurar(RestaurarSolicitudCommand cmd, CancellationToken cancellationToken);
}
