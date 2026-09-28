using COFIDE.WFF.Domain.ValueObjects;
namespace COFIDE.WFF.Application.Features.Usuarios.Ports.Out;

public interface IAutenticacionExternaPort
{
    Task<UsuarioAutenticado> AutenticarAsync(string codigoUsuario, CancellationToken cancellationToken);
    Task<bool> CerrarSesionAsync(CancellationToken cancellationToken = default);
}
