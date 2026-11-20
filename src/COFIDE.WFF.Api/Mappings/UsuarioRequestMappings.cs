using COFIDE.WFF.Api.Request.Usuarios;
using COFIDE.WFF.Application.Features.Usuarios.Commands.CambiarEstadoUsuario;
using COFIDE.WFF.Application.Features.Usuarios.Queries.ObtenerUsuariosPaginado;

namespace COFIDE.WFF.Api.Mappings;

/// <summary>Conversiones de solicitudes HTTP del módulo de usuarios.</summary>
public static class UsuarioRequestMappings
{
    /// <summary>Convierte los filtros y parámetros de paginación en una consulta de aplicación.</summary>
    /// <param name="request">Parámetros recibidos por el endpoint.</param>
    /// <returns>Consulta de usuarios paginados.</returns>
    public static ObtenerUsuariosPaginadoQuery ToQuery(this UsuarioPaginadoRequest request)
        => new(request.PageNumber, request.PageSize, request.Sorts, request.IdGerencia,
            request.IdDepartamento, request.IdCargo, request.Estado, request.IdUsuario);

    public static CambiarEstadoUsuarioCommand ToCommand(this CambiarEstadoUsuarioRequest request, long id)
        => new(id, request.Estado);
}
