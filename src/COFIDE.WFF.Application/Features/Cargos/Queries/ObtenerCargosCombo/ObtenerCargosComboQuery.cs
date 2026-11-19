using COFIDE.WFF.Application.Common.Queries;

namespace COFIDE.WFF.Application.Features.Cargos.Queries.ObtenerCargosCombo;

/// <summary>Solicita los cargos para un combo.</summary>
public sealed record ObtenerCargosComboQuery : BaseQuery<List<CargosComboDto>>;
