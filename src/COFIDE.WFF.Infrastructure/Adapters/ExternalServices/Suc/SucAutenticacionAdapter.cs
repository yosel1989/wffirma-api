using COFIDE.WFF.Application.Features.Usuarios.Ports.Out;
using COFIDE.WFF.Domain.ValueObjects;
using COFIDE.WFF.Infrastructure.Adapters.ExternalServices.Suc.DTOs;
using COFIDE.WFF.Infrastructure.Exceptions;
namespace COFIDE.WFF.Infrastructure.Adapters.ExternalServices.Suc;

public class SucAutenticacionAdapter(
        SucApiClient sucApiClient
    ) : IAutenticacionExternaPort
{
    private readonly SucApiClient _sucApiClient = sucApiClient;

    public async Task<UsuarioAutenticado> AutenticarAsync(string codigoUsuario, CancellationToken cancellationToken)
    {
        var sistemaTask = _sucApiClient.IngresarSistemaAsync(codigoUsuario, cancellationToken);
        var usuarioTask = _sucApiClient.ObtenerDatosUsuarioPorCodigo(codigoUsuario, cancellationToken);

        await Task.WhenAll(sistemaTask, usuarioTask);

        var sistema = await sistemaTask;
        var usuario = await usuarioTask;

        if (sistema is null || sistema.JwtToken is null)
            throw new InfrastructureException(SucErrors.AuthFailed);

        if (usuario is null)
            throw new InfrastructureException(SucErrors.UserDataMissing);

        var menus = sistema.lstMenu?
                    .Select(x => new MenuExternoDto(
                        x.Id,
                        x.IdMenuPadre,
                        x.Nombre,
                        x.Descripcion, x.DescripcionCompleta, x.Url, x.Icono
                    ))
                    .ToList() ?? [];

        var subMenus = sistema.lstSubMenu?
                    .Select(x => new MenuExternoDto(
                        x.Id,
                        x.IdMenuPadre,
                        x.Nombre, x.Descripcion, x.DescripcionCompleta, x.Url, x.Icono
                    ))
                    .ToList() ?? [];

        return new UsuarioAutenticado(
            sistema.CodigoSistema,
            sistema.NombreSistema,
            sistema.CodigoUsuario,
            sistema.NombreUsuario,
            sistema.CodigoRolUsuario,
            sistema.NombreRolUsuario,
            usuario?.Correo ?? string.Empty,
            sistema.JwtToken.Token,
            menus,
            subMenus);
    }

    public async Task<bool> CerrarSesionAsync(CancellationToken cancellationToken = default)
    {
        return await _sucApiClient.CerrarSesionAsync(cancellationToken);
    }

    public async Task<List<ObtenerParametroDetalleResponseDto>> ObtenerConfiguracionesAsync(string codigoConfiguracion, CancellationToken cancellationToken = default)
    {
        return await _sucApiClient.ObtenerParametroDetalleAsync(codigoConfiguracion, cancellationToken);
    }
}
