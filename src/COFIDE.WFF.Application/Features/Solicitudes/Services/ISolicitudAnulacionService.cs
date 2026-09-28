using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
namespace COFIDE.WFF.Application.Features.Solicitudes.Services;

public interface ISolicitudAnulacionService
{
    Task AnularSolicitudAsync(
        Solicitud solicitud,
        long idUsuario,
        CancellationToken cancellationToken);
}

