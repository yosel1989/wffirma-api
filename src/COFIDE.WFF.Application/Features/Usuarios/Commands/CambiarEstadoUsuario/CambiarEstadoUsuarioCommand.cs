using COFIDE.WFF.Application.Common.Commands;

namespace COFIDE.WFF.Application.Features.Usuarios.Commands.CambiarEstadoUsuario;

public sealed record CambiarEstadoUsuarioCommand(long IdUsuario, bool Estado)
    : UpdateCommand<bool, long>(IdUsuario);
