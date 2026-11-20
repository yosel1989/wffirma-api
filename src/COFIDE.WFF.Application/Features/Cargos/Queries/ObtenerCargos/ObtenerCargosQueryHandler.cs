using AutoMapper;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Features.Cargos.Ports.Out;

namespace COFIDE.WFF.Application.Features.Cargos.Queries.ObtenerCargos;

/// <summary>Gestiona la consulta del listado de cargos para su selección.</summary>
/// <param name="mapper">Servicio de mapeo requerido por el handler base.</param>
/// <param name="cargoRepository">Repositorio de cargos.</param>
public sealed class ObtenerCargosQueryHandler(
    IMapper mapper,
    ICargoRepository cargoRepository)
    : BaseQueryHandler<ObtenerCargosQuery, List<ObtenerCargosDto>>(mapper)
{
    /// <summary>Obtiene la lista de cargos desde el repositorio.</summary>
    /// <param name="request">Consulta del combo de cargos.</param>
    /// <param name="cancellationToken">Token de cancelación de la consulta.</param>
    /// <returns>Lista de cargos para el combo.</returns>
    public override Task<List<ObtenerCargosDto>> Handle(
        ObtenerCargosQuery request, CancellationToken cancellationToken)
        => cargoRepository.GetObtenerCargos(cancellationToken);
}
