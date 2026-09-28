using COFIDE.WFF.Application.Common.Queries;
namespace COFIDE.WFF.Application.Features.Areas.Queries.ObtenerDepartamentosCombo;
public sealed record ObtenerDepartamentosComboQuery(List<long> CodigoGerencia) : BaseQuery<List<DepartamentosComboDto>>;
