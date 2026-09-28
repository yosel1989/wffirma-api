using COFIDE.WFF.Application.Common.Queries;
namespace COFIDE.WFF.Application.Features.Maestros.Queries.ObtenerSubTipoSolicitudPorArea;

public sealed record ObtenerSubTipoSolicitudPorAreaQuery(
    List<long>? IdArea,
    List<long>? IdTipoSolicitud) : BaseQuery<List<SubTipoSolicitudComboDto>>;

