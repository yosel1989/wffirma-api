using COFIDE.WFF.Application.Features.AccesosEspeciales.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.AccesoEspecialAggregate;
using COFIDE.WFF.Infrastructure.Adapters.Repositories.Base;
using COFIDE.WFF.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

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

        #region Consultar

        public async Task<AccesoEspecial?> GetByIdAnyStatusAsync(long id, bool tracking)
        {
            IQueryable<AccesoEspecial> query = _context.AccesosEspeciales;

            if (!tracking)
                query = query.AsNoTracking();

            return await query
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        #endregion

        #region Eliminar

        public void Delete(AccesoEspecial accesoEspecial)
        {
            _context.AccesosEspeciales.Remove(accesoEspecial);
        }

        #endregion


    }
}
