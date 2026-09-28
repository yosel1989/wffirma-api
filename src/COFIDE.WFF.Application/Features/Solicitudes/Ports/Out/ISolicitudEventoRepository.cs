using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
namespace COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;

public interface ISolicitudEventoRepository : IGenericRepository<SolicitudEvento, long>
{
    Task<long> ObtenerSiguienteIdEventoAsync(CancellationToken cancellationToken);
    Task<List<long>> ObtenerBloqueSiguienteIdEventoAsync(long cantidad, CancellationToken cancellationToken);
}