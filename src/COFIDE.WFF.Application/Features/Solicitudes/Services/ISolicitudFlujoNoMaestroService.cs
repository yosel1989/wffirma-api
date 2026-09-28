using COFIDE.WFF.Application.Features.Solicitudes.DTOs;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
namespace COFIDE.WFF.Application.Features.Solicitudes.Services;

public interface ISolicitudFlujoNoMaestroService
{
    Task<Plantilla> CrearPlantillaDesdeSolicitud(CrearSolicitudPlantilla request, Solicitud solicitud, CancellationToken cancellationToken);
    Task CrearFlujoDesdeSolicitud(Plantilla plantilla, Solicitud solicitud, CancellationToken cancellationToken);

    Task<SolicitudFlujoDiff?> ActualizarPlantillaDesdeSolicitud(CrearSolicitudPlantilla request, Solicitud solicitud, Plantilla plantilla, CancellationToken cancellationToken);
    Task ActualizarFlujoDesdeSolicitud(Plantilla plantilla, Solicitud solicitud, CancellationToken cancellationToken);
}
