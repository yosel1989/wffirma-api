using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
using COFIDE.WFF.Application.Features.Maestros.Queries.ObtenerSubTipoSolicitudPorArea;
using COFIDE.WFF.Application.Features.Maestros.Queries.ObtenerTipoOperacionPorArea;
using COFIDE.WFF.Application.Features.Maestros.Queries.ObtenerTipoSolicitudPorArea;
using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Infrastructure.Adapters.Repositories.Base;
using COFIDE.WFF.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
namespace COFIDE.WFF.Infrastructure.Adapters.Repositories;

public class MaestroRepository(
        WorkflowFirmaDbContext context
    ) : GenericRepository<TipoOperacion, long>(context), IMaestroRepository
{
    public async Task<List<TipoOperacion>> GetByIdAreaAsync(long idArea, CancellationToken cancellationToken, bool tracking = false)
    {
        IQueryable<TipoOperacion> query = _context.Set<TipoOperacion>();

        if (!tracking)
            query = query.AsNoTracking();

        return await query
            .Include(u => u.Area)
            .Where(u =>
                u.IdArea == idArea
                && u.Estado)
            .OrderBy(ts => ts.NombreTipoOperacion)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TipoOperacionComboDto>> GetTipoOperacionCombo(List<long>? idArea, CancellationToken cancellationToken)
    {
        var query = _context.TipoOperacion
            .AsNoTracking()
            .AsQueryable();

        if (idArea != null && idArea.Count > 0)
            query = query.Where(x => idArea.Contains(x.IdArea));

        return await query
            .OrderBy(x => x.NombreTipoOperacion)
            .Select(x => new TipoOperacionComboDto
            {
                Id = x.Id,
                Nombre = x.NombreTipoOperacion
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<TipoSolicitud?> GetTipoSolicitudByIdAsync(long id, CancellationToken cancellationToken, bool tracking = false)
    {
        IQueryable<TipoSolicitud> query = _context.Set<TipoSolicitud>();

        if (!tracking)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(e => e.Id.Equals(id) && e.Estado, cancellationToken);
    }

    public async Task<List<TipoSolicitud>> GetTipoSolicitudByIdAreaAsync(long idArea, CancellationToken cancellationToken, bool tracking = false)
    {
        IQueryable<TipoSolicitud> query = _context.Set<TipoSolicitud>();

        if (!tracking)
            query = query.AsNoTracking();

        return await query
            .Include(u => u.Area)
            .Where(u =>
                u.IdArea == idArea
                && u.Estado)
            .OrderBy(ts => ts.NombreTipoSolicitud)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TipoSolicitudComboDto>> GetTipoSolicitudCombo(List<long>? idArea, CancellationToken cancellationToken)
    {
        var query = _context.TipoSolicitudes
            .AsNoTracking()
            .AsQueryable();

        if (idArea != null && idArea.Count > 0)
            query = query.Where(x => idArea.Contains(x.IdArea));

        return await query
            .OrderBy(x => x.NombreTipoSolicitud)
            .Select(x => new TipoSolicitudComboDto
            {
                Id = x.Id,
                Nombre = x.NombreTipoSolicitud
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<SubTipoSolicitud>> GetByIdTipoSolicitudAndIdAreaAsync(long idArea, long idTipoSolicitud, CancellationToken cancellationToken, bool tracking = false)
    {
        IQueryable<SubTipoSolicitud> query = _context.Set<SubTipoSolicitud>();

        if (!tracking)
            query = query.AsNoTracking();

        return await query
            .Include(u => u.TipoSolicitud)
            .Include(u => u.Area)
            .Where(u =>
                u.IdTipoSolicitud == idTipoSolicitud
                && u.IdArea == idArea
                && u.Estado)
            .OrderBy(ts => ts.NombreSubTipoSolicitud)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<SubTipoSolicitudComboDto>> GetSubTipoSolicitudCombo(List<long>? idArea, List<long>? idTipoSolicitud, CancellationToken cancellationToken)
    {
        var query = _context.SubTipoSolicitud
            .AsNoTracking()
            .AsQueryable();

        if (idArea != null && idArea.Count > 0)
            query = query.Where(x => idArea.Contains(x.IdArea));

        if (idTipoSolicitud != null && idTipoSolicitud.Count > 0)
            query = query.Where(x => idTipoSolicitud.Contains(x.IdTipoSolicitud));

        return await query
            .OrderBy(x => x.NombreSubTipoSolicitud)
            .Select(x => new SubTipoSolicitudComboDto
            {
                Id = x.Id,
                Nombre = x.NombreSubTipoSolicitud
            })
            .ToListAsync(cancellationToken);
    }
}
