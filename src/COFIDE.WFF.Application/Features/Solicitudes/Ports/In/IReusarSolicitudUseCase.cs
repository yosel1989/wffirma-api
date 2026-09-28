using COFIDE.WFF.Application.Features.Solicitudes.DTOs;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
namespace COFIDE.WFF.Application.Features.Solicitudes.Ports.In;

public interface IReusarSolicitudUseCase
{
    Task<(Solicitud, Plantilla?)> Reusar(SolicitudRequestDto cmd, long idSolicitudOrigen, bool esBorrador, CancellationToken cancellationToken);
}
