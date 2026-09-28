using COFIDE.WFF.Application.Features.Solicitudes.Commands.AprobarSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.RechazarSolicitud;
namespace COFIDE.WFF.Application.Features.Solicitudes.Ports.In;

public interface IRevisarSolicitudUseCase
{
    Task<string?> Aprobar(AprobarSolicitudCommand cmd, CancellationToken cancellationToken);
    Task<string?> Rechazar(RechazarSolicitudCommand cmd, CancellationToken cancellationToken);
}
