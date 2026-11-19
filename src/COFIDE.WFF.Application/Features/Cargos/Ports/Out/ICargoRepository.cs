using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Cargos.Queries.ObtenerCargosCombo;
using COFIDE.WFF.Domain.Entities;

namespace COFIDE.WFF.Application.Features.Cargos.Ports.Out;

/// <summary>Contrato de acceso a los cargos.</summary>
public interface ICargoRepository : IGenericRepository<Cargo, long>
{
    /// <summary>Obtiene todos los cargos ordenados por nombre para un combo.</summary>
    /// <param name="cancellationToken">Token de cancelación de la consulta.</param>
    /// <returns>Lista de identificadores y nombres de cargos.</returns>
    Task<List<CargosComboDto>> GetCargoCombo(CancellationToken cancellationToken);
}
