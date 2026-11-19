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
    Task<List<UsuariosComboDto>> GetUsuarioCombo(CancellationToken cancellationToken);

    /// <summary>Obtiene una página de usuarios y el total de coincidencias de los filtros.</summary>
    /// <param name="pagination">Parámetros de paginación y ordenamiento.</param>
    /// <param name="idGerencia">Identificador opcional de la gerencia.</param>
    /// <param name="idDepartamento">Identificador opcional del departamento.</param>
    /// <param name="idCargo">Identificador opcional del cargo.</param>
    /// <param name="estado">Estado opcional del usuario: activo o inactivo.</param>
    /// <param name="idUsuario">Identificador opcional del usuario.</param>
    /// <param name="cancellationToken">Token de cancelación de la consulta.</param>
    /// <returns>Resultados y metadatos de la página de usuarios.</returns>
    Task<PagedResult<UsuarioPaginadoDto>> ObtenerPaginadoAsync(
        PagedQueryParams pagination, long? idGerencia, long? idDepartamento, long? idCargo,
        bool? estado, long? idUsuario, CancellationToken cancellationToken);
}