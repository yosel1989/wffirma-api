using COFIDE.WFF.Application.Common.Pageds;
using MediatR;

namespace COFIDE.WFF.Application.Features.Usuarios.Queries.ObtenerUsuariosPaginado;

/// <summary>Solicita una página de usuarios con filtros opcionales.</summary>
/// <param name="PageNumber">Número de página, a partir de uno.</param>
/// <param name="PageSize">Cantidad de usuarios por página.</param>
/// <param name="Sorts">Campos y direcciones de ordenamiento.</param>
/// <param name="IdGerencia">Gerencias opcionales; se ignoran si se envía al menos un departamento.</param>
/// <param name="IdDepartamento">Departamentos opcionales; tienen prioridad sobre las gerencias.</param>
/// <param name="IdCargo">Identificador opcional del cargo.</param>
/// <param name="Estado">Estado opcional del usuario: activo o inactivo.</param>
/// <param name="IdUsuario">Identificador opcional del usuario.</param>
public sealed record ObtenerUsuariosPaginadoQuery(
    int PageNumber,
    int PageSize,
    IReadOnlyList<SortDto>? Sorts,
    IReadOnlyList<long>? IdGerencia,
    IReadOnlyList<long>? IdDepartamento,
    long? IdCargo,
    bool? Estado,
    long? IdUsuario) : IRequest<PagedResultDto<UsuarioPaginadoDto>>;
