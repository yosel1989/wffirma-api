using COFIDE.WFF.Application.Common.Queries;
namespace COFIDE.WFF.Application.Features.Maestros.Queries.ObtenerTipoSolicitudPorArea;
public sealed record ObtenerTipoSolicitudPorAreaQuery(List<long>? IdArea) : BaseQuery<List<TipoSolicitudComboDto>>;