using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Infrastructure.Adapters.Repositories.Base;
using COFIDE.WFF.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
namespace COFIDE.WFF.Infrastructure.Adapters.Repositories;

public class SolicitudEventoRepository(
        WorkflowFirmaDbContext context
    ) : GenericRepository<SolicitudEvento, long>(context), ISolicitudEventoRepository
{
    public async Task<long> ObtenerSiguienteIdEventoAsync(CancellationToken cancellationToken)
    {
        var maxId = await _context.Set<SolicitudEvento>()
        .MaxAsync(x => (long?)x.Id, cancellationToken) ?? 0;

        return maxId + 1;
    }

    public async Task<List<long>> ObtenerBloqueSiguienteIdEventoAsync(long cantidad, CancellationToken cancellationToken)
    {
        var maxId = await _context.Set<SolicitudEvento>()
        .MaxAsync(x => (long?)x.Id, cancellationToken) ?? 0;

        var result = new List<long>();

        if (cantidad <= 0)
            return result;

        for (var i = 1; i <= cantidad; i++)
        {
            result.Add(maxId + i);
        }

        return result;
    }
}