using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Usuarios.Commands.CambiarEstadoUsuario.Errors;
using COFIDE.WFF.Application.Features.Usuarios.Ports.Out;
using COFIDE.WFF.Domain.Common.Enums;
using MediatR;
using System.Net;
using ApplicationException = COFIDE.WFF.Application.Exceptions.ApplicationException;

namespace COFIDE.WFF.Application.Features.Usuarios.Commands.CambiarEstadoUsuario;

public sealed class CambiarEstadoUsuarioHandler(
    ICurrentUserService currentUser,
    IUsuarioRepository usuarioRepository)
    : IRequestHandler<CambiarEstadoUsuarioCommand, bool>
{
    public async Task<bool> Handle(CambiarEstadoUsuarioCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.Autenticado || currentUser.UserIdLocal is null || currentUser.UserCargoId is null)
            throw new ApplicationException(ApplicationErrors.UserNotAuthenticated with
            {
                HttpStatusCode = (int)HttpStatusCode.Unauthorized
            });

        if (RolUsuarioCodigo.Desde(currentUser.CodigoRolUsuario) != RolUsuario.Administrador)
            throw new ApplicationException(ApplicationErrors.Forbidden with
            {
                HttpStatusCode = (int)HttpStatusCode.Forbidden
            });

        var usuario = await usuarioRepository.GetByIdAnyStatusAsync(request.Id, true, cancellationToken);
        if (usuario is null)
            throw new ApplicationException(CambiarEstadoUsuarioErrors.UsuarioNoExiste);

        if (request.Estado)
            usuario.Activar();
        else
            usuario.Desactivar();

        await usuarioRepository.UpdateAsync(usuario);
        return usuario.Estado;
    }
}
