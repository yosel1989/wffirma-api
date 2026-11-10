using COFIDE.WFF.Application.Features.Usuarios.Queries.ObtenerUsuariosCombo;
using COFIDE.WFF.Application.Features.Usuarios.Queries.UsuarioBuscador;
using COFIDE.WFF.Domain.Entities;
namespace COFIDE.WFF.Application.Features.Usuarios.Ports.Out;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByIdAsync(long idUsuario, bool tracking = false);
    Task<Usuario?> GetByCuentaLogueoAsync(string cuentaLogueo, bool tracking = false);
    Task<List<UsuarioBuscadorDto>> BuscarAsync(string termino, CancellationToken cancellationToken);
    Task<List<UsuariosComboDto>> GetUsuarioCombo(CancellationToken cancellationToken);
}