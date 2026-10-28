using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerPuntosRestauracion;

namespace COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;

public interface ISolicitudRestauracionFlujoQueryRepository
{
    Task<IReadOnlyList<PuntoRestauracionDto>> ObtenerPuntosRestauracion(
        long idSolicitud,
        CancellationToken cancellationToken);
}
