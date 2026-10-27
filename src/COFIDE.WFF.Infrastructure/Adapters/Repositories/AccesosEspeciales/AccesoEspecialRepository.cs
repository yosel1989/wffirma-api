using COFIDE.WFF.Application.Features.AccesosEspeciales.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.AccesoEspecialAggregate;
using COFIDE.WFF.Infrastructure.Adapters.Repositories.Base;
using COFIDE.WFF.Infrastructure.Persistence.Contexts;

namespace COFIDE.WFF.Infrastructure.Adapters.Repositories.AccesosEspeciales
{
    public class AccesoEspecialRepository(
        WorkflowFirmaDbContext context
    ) : GenericRepository<AccesoEspecial, long>(context), IAccesoEspecialRepository
    {

        #region Registrar

        public async void RegistrarAccesoEspecialAsync(AccesoEspecial accesoEspecial)
        {
            await _context.AccesosEspeciales.AddAsync(accesoEspecial);
            await _context.SaveChangesAsync();

        }

        #endregion
    }
}
