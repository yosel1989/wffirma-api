using COFIDE.WFF.Application.Features.Usuarios.Models;

namespace COFIDE.WFF.Application.Features.Usuarios.Ports.Out;
public interface IUsuarioIdentityResolver
{
    Task<UsuarioIdentitySnapshot?> ResolveAsync(string codigoUsuario, TimeSpan cacheTtl, string jti);
}
