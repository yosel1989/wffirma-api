using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerEstadoSolicitudCombo;
using COFIDE.WFF.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
namespace COFIDE.WFF.Infrastructure.Adapters.Repositories;

public class EstadoSolicitudQueryRepository(
        WorkflowFirmaDbContext context
    ) : IEstadoSolicitudQueryRepository
{
    private readonly WorkflowFirmaDbContext _context= context;
    public async Task<List<EstadoSolicitudComboDto>> GetEstadoSolicitudCombo(CancellationToken cancellationToken)
    {
        var estadoSolicitud = await _context.EstadoSolicitud
            .AsNoTracking()
            .OrderBy(u => u.NombreEstadoSolicitud)
            .Select(u => new EstadoSolicitudComboDto
            {
                Id = u.Id,
                Nombre = u.NombreEstadoSolicitud ?? string.Empty
            })
            .ToListAsync(cancellationToken);

        return estadoSolicitud;
    }
}
