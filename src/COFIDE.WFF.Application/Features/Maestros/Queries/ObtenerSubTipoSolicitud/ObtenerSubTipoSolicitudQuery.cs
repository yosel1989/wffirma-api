using COFIDE.WFF.Application.Common.Queries;
namespace COFIDE.WFF.Application.Features.Maestros.Queries.ObtenerSubTipoSolicitud;

public sealed record ObtenerSubTipoSolicitudQuery(long IdTipoSolicitud) : BaseQuery<List<SubTipoSolicitudDto>>;