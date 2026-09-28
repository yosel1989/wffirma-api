using COFIDE.WFF.Application.Features.Solicitudes.DTOs;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
namespace COFIDE.WFF.Application.Features.Solicitudes.Ports.In;

public interface ICrearSolicitudUseCase
{
    Task<(Solicitud, Plantilla?)> Crear(SolicitudRequestDto cmd, bool esBorrador, CancellationToken cancellationToken);
}
