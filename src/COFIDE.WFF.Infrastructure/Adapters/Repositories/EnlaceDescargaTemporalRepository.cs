using COFIDE.WFF.Application.Features.Publico.Ports.Out;
using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Infrastructure.Adapters.Repositories.Base;
using COFIDE.WFF.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
namespace COFIDE.WFF.Infrastructure.Adapters.Repositories;

public class EnlaceDescargaTemporalRepository(
        WorkflowFirmaDbContext context
    ) : GenericRepository<EnlaceDescargaTemporal, long>(context), IEnlaceDescargaTemporalRepository
{

    public async Task<EnlaceDescargaTemporal?> ObtenerPorIdSolicitudAsync(long idSolicitud, CancellationToken cancellationToken)
    {
        return await _context.EnlaceDescargaTemporales
       .FirstOrDefaultAsync(x => x.IdSolicitud == idSolicitud, cancellationToken);
    }

    public async Task<EnlaceDescargaTemporal?> ObtenerPorTokenAsync(string token, CancellationToken cancellationToken)
    {
        return await _context.EnlaceDescargaTemporales
       .FirstOrDefaultAsync(x => x.Token == token, cancellationToken);
    }
}
