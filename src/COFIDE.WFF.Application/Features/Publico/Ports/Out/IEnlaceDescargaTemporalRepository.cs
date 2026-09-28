using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Domain.Entities;
namespace COFIDE.WFF.Application.Features.Publico.Ports.Out;

public interface IEnlaceDescargaTemporalRepository : IGenericRepository<EnlaceDescargaTemporal, long>
{
    Task<EnlaceDescargaTemporal?> ObtenerPorIdSolicitudAsync(long idSolicitud, CancellationToken cancellationToken);
    Task<EnlaceDescargaTemporal?> ObtenerPorTokenAsync(string token, CancellationToken cancellationToken);
}
