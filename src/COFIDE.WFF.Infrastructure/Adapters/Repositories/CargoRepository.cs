using COFIDE.WFF.Application.Features.Cargos.Ports.Out;
using COFIDE.WFF.Application.Features.Cargos.Queries.ObtenerCargos;
using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Infrastructure.Adapters.Repositories.Base;
using COFIDE.WFF.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace COFIDE.WFF.Infrastructure.Adapters.Repositories;

/// <summary>Acceso a los cargos almacenados.</summary>
/// <param name="context">Contexto de persistencia.</param>
public class CargoRepository(WorkflowFirmaDbContext context)
    : GenericRepository<Cargo, long>(context), ICargoRepository
{
    /// <inheritdoc />
    public async Task<List<ObtenerCargosDto>> ObtenerCargos(CancellationToken cancellationToken)
        => await _context.Set<Cargo>()
            .AsNoTracking()
            .OrderBy(cargo => cargo.NombreCargo)
            .Select(cargo => new ObtenerCargosDto
            {
                IdCargo = cargo.Id,
                NombreCargo = cargo.NombreCargo
            })
            .ToListAsync(cancellationToken);
}
