using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Domain.Entities;
namespace COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;

public interface IFormatoNotificacionRepository : IGenericRepository<FormatoNotificacion, long>
{
    Task<FormatoNotificacion?> ObtenerPorCodigo(string nombreFormato, CancellationToken cancellationToken);
}
