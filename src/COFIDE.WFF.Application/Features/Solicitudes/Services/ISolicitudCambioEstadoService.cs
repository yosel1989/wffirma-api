using COFIDE.WFF.Application.Features.Solicitudes.Commands.CambiarEstadoSolicitud;

namespace COFIDE.WFF.Application.Features.Solicitudes.Services;

public interface ISolicitudCambioEstadoService
{
    Task<string?> CambiarEstado(CambiarEstadoSolicitudCommand cmd, CancellationToken cancellationToken);
}
