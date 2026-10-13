using COFIDE.WFF.Application.Features.Solicitudes.DTOs;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
namespace COFIDE.WFF.Application.Features.Solicitudes.Services;

/// <summary>Crea una solicitud a partir de otra. Lo comparten los handlers de reusar y de reusar y enviar.</summary>
public interface ISolicitudReusoService
{
    Task<(Solicitud, Plantilla?)> Reusar(SolicitudRequestDto cmd, long idSolicitudOrigen, bool esBorrador, CancellationToken cancellationToken);
}
