using COFIDE.WFF.Application.Common.Queries;

namespace COFIDE.WFF.Application.Features.Cargos.Queries.ObtenerCargos;

/// <summary>Solicita los cargos para su selección.</summary>
public sealed record ObtenerCargosQuery : BaseQuery<List<ObtenerCargosDto>>;
