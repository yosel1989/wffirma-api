using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Infrastructure.Adapters.Repositories.Base;
using COFIDE.WFF.Infrastructure.Persistence.Contexts;
namespace COFIDE.WFF.Infrastructure.Adapters.Repositories;

public class MovimientoSolicitudRepository(
        WorkflowFirmaDbContext context
    ) : GenericRepository<MovimientoSolicitud, long>(context), IMovimientoSolicitudRepository
{ }