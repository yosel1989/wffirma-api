using COFIDE.WFF.Application.Features.Solicitudes.DTOs;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
namespace COFIDE.WFF.Application.Features.Solicitudes.Ports.In;

public interface IEditarSolicitudUseCase
{
    Task<(Solicitud solicitud, Plantilla? plantilla)> Editar(SolicitudRequestDto cmd, long idSolicitud, bool esBorrador, CancellationToken cancellationToken);
}
