using AutoMapper;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Features.Cargos.Ports.Out;

namespace COFIDE.WFF.Application.Features.Cargos.Queries.ObtenerCargosCombo;

/// <summary>Gestiona la consulta del combo de cargos.</summary>
/// <param name="mapper">Servicio de mapeo requerido por el handler base.</param>
/// <param name="cargoRepository">Repositorio de cargos.</param>
public sealed class ObtenerCargosComboQueryHandler(
    IMapper mapper,
    ICargoRepository cargoRepository)
    : BaseQueryHandler<ObtenerCargosComboQuery, List<CargosComboDto>>(mapper)
{
    /// <summary>Obtiene la lista de cargos desde el repositorio.</summary>
    /// <param name="request">Consulta del combo de cargos.</param>
    /// <param name="cancellationToken">Token de cancelación de la consulta.</param>
    /// <returns>Lista de cargos para el combo.</returns>
    public override Task<List<CargosComboDto>> Handle(
        ObtenerCargosComboQuery request, CancellationToken cancellationToken)
        => cargoRepository.GetCargoCombo(cancellationToken);
}
