using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerEstadoSolicitudCombo;
namespace COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;

public interface IEstadoSolicitudQueryRepository
{
    Task<List<EstadoSolicitudComboDto>> GetEstadoSolicitudCombo(CancellationToken cancellationToken);
}

