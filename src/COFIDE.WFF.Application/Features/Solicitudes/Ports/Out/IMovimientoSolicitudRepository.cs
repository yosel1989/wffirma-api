using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
namespace COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;

public interface IMovimientoSolicitudRepository : IGenericRepository<MovimientoSolicitud, long> { }
