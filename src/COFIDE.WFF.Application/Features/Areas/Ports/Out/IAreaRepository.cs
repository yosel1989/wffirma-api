using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Areas.Queries.ObtenerAreasCombo;
using COFIDE.WFF.Application.Features.Areas.Queries.ObtenerDepartamentosCombo;
using COFIDE.WFF.Domain.Entities;
namespace COFIDE.WFF.Application.Features.Areas.Ports.Out;

public interface IAreaRepository : IGenericRepository<Area, long> {
    Task<List<AreasComboDto>> GetAreaCombo(char? Tipo, CancellationToken cancellationToken);
    Task<List<DepartamentosComboDto>> GetDepartamentosCombo(List<long> codigoGerencia, CancellationToken cancellationToken);
}
