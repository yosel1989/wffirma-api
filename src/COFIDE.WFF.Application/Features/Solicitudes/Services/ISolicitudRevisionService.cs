using COFIDE.WFF.Application.Features.Solicitudes.Commands.AprobarSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.RechazarSolicitud;
namespace COFIDE.WFF.Application.Features.Solicitudes.Services;

/// <summary>Aprobación y rechazo de solicitudes; comparten la preparación del aprobador y el registro de sustentos.</summary>
public interface ISolicitudRevisionService
{
    Task<string?> Aprobar(AprobarSolicitudCommand cmd, CancellationToken cancellationToken);
    Task<string?> Rechazar(RechazarSolicitudCommand cmd, CancellationToken cancellationToken);
}
