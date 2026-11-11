using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;

namespace COFIDE.WFF.Application.Features.Solicitudes.Services;

public interface ISolicitudConsultaAuthorizationService
{
    Task AutorizarPorIdAsync(long idSolicitud, CancellationToken cancellationToken);
    Task AutorizarReusoPorIdAsync(long idSolicitud, CancellationToken cancellationToken);
    Task AutorizarSolicitudAsync(Solicitud? solicitud);
}
