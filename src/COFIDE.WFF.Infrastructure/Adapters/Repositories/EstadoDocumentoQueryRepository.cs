using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerEstadoDocumentosCombo;
using COFIDE.WFF.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
namespace COFIDE.WFF.Infrastructure.Adapters.Repositories;

public class EstadoDocumentoQueryRepository(
        WorkflowFirmaDbContext context
    ) : IEstadoDocumentoQueryRepository
{
    private readonly WorkflowFirmaDbContext _context = context;

    public async Task<List<EstadoDocumentosComboDto>> GetEstadoDocumentoCombo(CancellationToken cancellationToken)
    {
        var estadoDocumento = await _context.EstadoDocumento
                    .AsNoTracking()
                    .OrderBy(u => u.NombreEstadoDocumento)
                    .Select(u => new EstadoDocumentosComboDto
                    {
                        Id = u.Id,
                        Nombre = u.NombreEstadoDocumento
                    })
                    .ToListAsync(cancellationToken);

        return estadoDocumento;
    }
}

