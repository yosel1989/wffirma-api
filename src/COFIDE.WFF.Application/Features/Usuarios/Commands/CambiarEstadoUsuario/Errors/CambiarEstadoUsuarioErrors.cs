using COFIDE.WFF.Application.Common.Errors;
using System.Net;

namespace COFIDE.WFF.Application.Features.Usuarios.Commands.CambiarEstadoUsuario.Errors;

public static class CambiarEstadoUsuarioErrors
{
    public static readonly ApplicationError UsuarioNoExiste = new(
        "USUARIO_NO_EXISTE", "El usuario no existe.", (int)HttpStatusCode.NotFound);
}
