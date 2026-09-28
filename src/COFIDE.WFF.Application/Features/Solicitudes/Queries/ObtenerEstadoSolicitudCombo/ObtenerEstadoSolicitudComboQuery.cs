using COFIDE.WFF.Application.Common.Queries;
namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerEstadoSolicitudCombo;

public sealed record ObtenerEstadoSolicitudComboQuery() : BaseQuery<List<EstadoSolicitudComboDto>>;