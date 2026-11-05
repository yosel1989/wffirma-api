using COFIDE.WFF.Application.Common.Queries;
namespace COFIDE.WFF.Application.Features.Areas.Queries.ObtenerAreasCombo;
public sealed record ObtenerAreasComboQuery(char? Tipo, long? excluirAreaUsuario) : BaseQuery<List<AreasComboDto>>;
