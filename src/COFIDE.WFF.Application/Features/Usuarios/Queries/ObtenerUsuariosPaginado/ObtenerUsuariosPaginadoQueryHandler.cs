using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Pageds;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Usuarios.Ports.Out;
using COFIDE.WFF.Domain.Common.Enums;
using MediatR;
using System.Net;
using ApplicationException = COFIDE.WFF.Application.Exceptions.ApplicationException;

namespace COFIDE.WFF.Application.Features.Usuarios.Queries.ObtenerUsuariosPaginado;

/// <summary>Obtiene el listado paginado de usuarios para un administrador autenticado.</summary>
/// <param name="currentUser">Servicio que proporciona el usuario de la solicitud.</param>
/// <param name="usuarioRepository">Repositorio de usuarios.</param>
public sealed class ObtenerUsuariosPaginadoQueryHandler(
    ICurrentUserService currentUser,
    IUsuarioRepository usuarioRepository)
    : IRequestHandler<ObtenerUsuariosPaginadoQuery, PagedResultDto<UsuarioPaginadoDto>>
{
    /// <summary>Comprueba el acceso y devuelve la página de usuarios solicitada.</summary>
    /// <param name="request">Filtros, orden y parámetros de paginación.</param>
    /// <param name="cancellationToken">Token de cancelación de la consulta.</param>
    /// <returns>Página de usuarios con el total de coincidencias.</returns>
    public async Task<PagedResultDto<UsuarioPaginadoDto>> Handle(
        ObtenerUsuariosPaginadoQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.UserIdLocal is null || currentUser.UserCargoId is null)
            throw new ApplicationException(ApplicationErrors.UserNotAuthenticated with
            {
                HttpStatusCode = (int)HttpStatusCode.Unauthorized
            });

        if (RolUsuarioCodigo.Desde(currentUser.CodigoRolUsuario) != RolUsuario.Administrador)
            throw new ApplicationException(ApplicationErrors.Forbidden with
            {
                HttpStatusCode = (int)HttpStatusCode.Forbidden
            });

        var pagination = new PagedQueryParams
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            Sorts = request.Sorts?.Select(sort => (sort.PropertyName, sort.Descending)).ToList()
        };

        var result = await usuarioRepository.ObtenerPaginadoAsync(pagination, request.IdGerencia,
            request.IdDepartamento, request.IdCargo, request.Estado, request.IdUsuario, cancellationToken);

        return PagedResultDto<UsuarioPaginadoDto>.Create(
            result.Items, result.PageNumber, result.PageSize, result.TotalCount);
    }
}
