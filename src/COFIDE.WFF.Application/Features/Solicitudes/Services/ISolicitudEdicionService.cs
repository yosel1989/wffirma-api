using COFIDE.WFF.Application.Features.Solicitudes.DTOs;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
namespace COFIDE.WFF.Application.Features.Solicitudes.Services;

/// <summary>Edita una solicitud. Lo comparten los handlers de editar y de editar y enviar.</summary>
public interface ISolicitudEdicionService
{
    Task<(Solicitud solicitud, Plantilla? plantilla)> Editar(SolicitudRequestDto cmd, long idSolicitud, bool esBorrador, CancellationToken cancellationToken);
}
