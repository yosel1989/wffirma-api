using COFIDE.WFF.Application.Features.Areas.Ports.Out;
using COFIDE.WFF.Application.Features.Areas.Queries.ObtenerAreasCombo;
using COFIDE.WFF.Application.Features.Areas.Queries.ObtenerDepartamentosCombo;
using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Infrastructure.Adapters.Repositories.Base;
using COFIDE.WFF.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
namespace COFIDE.WFF.Infrastructure.Adapters.Repositories;
public class AreaRepository(
        WorkflowFirmaDbContext context
    ) : GenericRepository<Area, long>(context), IAreaRepository
{
    public async Task<List<AreasComboDto>> GetAreaCombo(List<long>? excluirIds, char? Tipo, CancellationToken cancellationToken)
    {
        var query = _context.Areas
            .AsQueryable();

        if (Tipo.HasValue && Tipo.Value == 'G')
        {
            query = query.Where(u => u.Tipo == Tipo.Value);
        }

        if (excluirIds?.Count > 0)
        {
            query = query.Where(u => !excluirIds.Contains(u.Id));
        }

        return await query
            .AsNoTracking()
            .OrderBy(u => u.NombreArea)
            .Select(u => new AreasComboDto
            {
                IdArea = u.Id,
                NombreArea = u.NombreArea
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DepartamentosComboDto>> GetDepartamentosCombo(List<long> codigoGerencia, CancellationToken cancellationToken)
    {
        if (codigoGerencia == null || codigoGerencia.Count == 0)
            return [];

        var departamentos = await _context.Areas
            .AsNoTracking()
            .Where(u =>
                u.Tipo == 'D' &&
                u.AreaPadre != null &&
                codigoGerencia.Contains(u.AreaPadre.Id))
            .OrderBy(u => u.NombreArea)
            .Select(u => new DepartamentosComboDto
            {
                IdDepartamento = u.Id,
                NombreDepartamento = u.NombreArea
            })
            .ToListAsync(cancellationToken);

        return departamentos;
    }
}
