using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
namespace COFIDE.WFF.Application.Features.Solicitudes.Services;

public interface ISolicitudPlantillaService
{
    Task<Plantilla?> CrearSolicitudPlantilla(long? idPlantillaOrigen, string? configuracion, long? idPlantillaRequest, long idUsuario, string gerenciaUsuario, CancellationToken cancellationToken);
    Task ActualizarFlujoDesdeSolicitud(Plantilla? plantilla, Solicitud solicitud, CancellationToken cancellationToken);
    Task CrearFlujoDesdeSolicitud(Plantilla? plantilla, Solicitud solicitud, CancellationToken cancellationToken);
}
