using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Infrastructure.Adapters.Repositories.Base;
using COFIDE.WFF.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
namespace COFIDE.WFF.Infrastructure.Adapters.Repositories;

public class FormatoNotificacionRepository(
        WorkflowFirmaDbContext context
    ) : GenericRepository<FormatoNotificacion, long>(context), IFormatoNotificacionRepository
{
    public async Task<FormatoNotificacion?> ObtenerPorCodigo(string nombreFormato, CancellationToken cancellationToken)
    {
        var nombreFormateado = nombreFormato.Trim().ToUpper();

        return await _context.FormatoNotificaciones
            .Where(x => (x.NombreFormato ?? string.Empty).Trim().ToUpper() == nombreFormateado)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
