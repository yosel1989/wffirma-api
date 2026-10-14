using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Maestros.Queries.ObtenerSubTipoSolicitudPorArea;
using COFIDE.WFF.Application.Features.Maestros.Queries.ObtenerTipoOperacionPorArea;
using COFIDE.WFF.Application.Features.Maestros.Queries.ObtenerTipoSolicitudPorArea;
using COFIDE.WFF.Domain.Entities;
namespace COFIDE.WFF.Application.Features.Maestros.Ports.Out;

public interface IMaestroRepository : IGenericRepository<TipoOperacion, long>
{
    Task<List<TipoOperacion>> GetByIdAreaAsync(long idArea, char Estado, CancellationToken cancellationToken, bool tracking = false);
    Task<List<TipoOperacionComboDto>> GetTipoOperacionCombo(List<long>? idArea, CancellationToken cancellationToken);

    Task<TipoSolicitud?> GetTipoSolicitudByIdAsync(long id, CancellationToken cancellationToken, bool tracking = false);
    Task<List<TipoSolicitud>> GetTipoSolicitudByIdAreaAsync(long idArea, char Estado, CancellationToken cancellationToken, bool tracking = false);
    Task<List<TipoSolicitudComboDto>> GetTipoSolicitudCombo(List<long>? idArea, CancellationToken cancellationToken);

    Task<List<SubTipoSolicitud>> GetByIdTipoSolicitudAndIdAreaAsync(long idArea, long idTipoSolicitud, char Estado, CancellationToken cancellationToken, bool tracking = false);
    Task<List<SubTipoSolicitudComboDto>> GetSubTipoSolicitudCombo(List<long>? idArea, List<long>? idTipoSolicitud, CancellationToken cancellationToken);
}