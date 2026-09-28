using COFIDE.WFF.Application.Common.Queries;
namespace COFIDE.WFF.Application.Features.Maestros.Queries.ObtenerTipoOperacionPorArea;
public sealed record ObtenerTipoOperacionPorAreaQuery(List<long>? IdArea) : BaseQuery<List<TipoOperacionComboDto>>;
