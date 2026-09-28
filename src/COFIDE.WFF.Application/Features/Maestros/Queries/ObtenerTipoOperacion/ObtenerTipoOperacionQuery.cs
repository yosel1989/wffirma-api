using COFIDE.WFF.Application.Common.Queries;
namespace COFIDE.WFF.Application.Features.Maestros.Queries.ObtenerTipoOperacion;

public sealed record ObtenerTipoOperacionQuery() : BaseQuery<List<TipoOperacionDto>>;