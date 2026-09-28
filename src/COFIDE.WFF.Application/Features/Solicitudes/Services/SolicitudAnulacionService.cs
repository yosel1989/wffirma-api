using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
namespace COFIDE.WFF.Application.Features.Solicitudes.Services;

public sealed class SolicitudAnulacionService : ISolicitudAnulacionService
{
    public Task AnularSolicitudAsync(
        Solicitud solicitud,
        long idUsuario,
        CancellationToken cancellationToken)
    {
        solicitud.Anular(idUsuario);
        return Task.CompletedTask;
    }
}

