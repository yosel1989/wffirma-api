using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Queries.ObtenerPorId;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
namespace COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;

public interface ISolicitudPlantillaRepository : IGenericRepository<SolicitudPlantilla, long>
{
    Task<PlantillaEtapaDto?> ObtenerEtapaActual(long IdSolicitud, long? IdSolicitudPlantilla, CancellationToken cancellationToken);
}