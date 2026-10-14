using COFIDE.WFF.Application.Common.Queries;
namespace COFIDE.WFF.Application.Features.Maestros.Queries.ObtenerTipoSolicitud;

public sealed record ObtenerTipoSolicitudQuery(char Estado) : BaseQuery<List<TipoSolicitudDto>>;