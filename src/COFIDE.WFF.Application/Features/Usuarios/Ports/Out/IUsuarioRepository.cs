using COFIDE.WFF.Application.Common.Pageds;
using COFIDE.WFF.Application.Features.Usuarios.Queries.ObtenerUsuariosCombo;
using COFIDE.WFF.Application.Features.Usuarios.Queries.ObtenerUsuariosPaginado;
using COFIDE.WFF.Application.Features.Usuarios.Queries.UsuarioBuscador;
using COFIDE.WFF.Domain.Entities;
namespace COFIDE.WFF.Application.Features.Usuarios.Ports.Out;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByIdAsync(long idUsuario, bool tracking = false);
    Task<Usuario?> GetByCuentaLogueoAsync(string cuentaLogueo, bool tracking = false);
    Task<List<UsuarioBuscadorDto>> BuscarAsync(string termino, CancellationToken cancellationToken);
    /// <summary>Obtiene el combo de usuarios.</summary>
    /// <param name="todos">Si se envía (con cualquier valor), incluye usuarios inactivos; si es <c>null</c>, solo activos.</param>
    /// <param name="cancellationToken">Token de cancelación de la consulta.</param>
    Task<List<UsuariosComboDto>> GetUsuarioCombo(bool? todos, CancellationToken cancellationToken);

    /// <summary>Obtiene una página de usuarios y el total de coincidencias de los filtros.</summary>
    /// <param name="pagination">Parámetros de paginación y ordenamiento.</param>
    /// <param name="idGerencia">Gerencias opcionales; incluye usuarios de la gerencia o de sus áreas hijas. Se ignoran si se envía al menos un departamento.</param>
    /// <param name="idDepartamento">Departamentos opcionales; si contiene elementos, solo se filtra por ellos.</param>
    /// <param name="idCargo">Identificador opcional del cargo.</param>
    /// <param name="estado">Estado opcional del usuario: activo o inactivo.</param>
    /// <param name="idUsuario">Identificador opcional del usuario.</param>
    /// <param name="cancellationToken">Token de cancelación de la consulta.</param>
    /// <returns>Resultados y metadatos de la página de usuarios.</returns>
    Task<PagedResult<UsuarioPaginadoDto>> ObtenerPaginadoAsync(
        PagedQueryParams pagination, IReadOnlyCollection<long>? idGerencia, IReadOnlyCollection<long>? idDepartamento, long? idCargo,
        bool? estado, long? idUsuario, CancellationToken cancellationToken);
}