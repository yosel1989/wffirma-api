using COFIDE.WFF.Application.Features.Usuarios.Models;
using COFIDE.WFF.Application.Features.Usuarios.Ports.Out;
using COFIDE.WFF.Domain.Common.Enums;
using COFIDE.WFF.Infrastructure.Adapters.ExternalServices.Suc;
using Microsoft.Extensions.Caching.Memory;
namespace COFIDE.WFF.Infrastructure.Security;
public class UsuarioIdentityResolver(
        IUsuarioRepository usuarioQueryRepository,
        SucApiClient sucApiClient,
        IMemoryCache cache
    ) : IUsuarioIdentityResolver
{
    private readonly IUsuarioRepository _usuarioQueryRepository = usuarioQueryRepository;
    private readonly SucApiClient _sucApiClient = sucApiClient;
    private readonly IMemoryCache _cache = cache;

    public async Task<UsuarioIdentitySnapshot?> ResolveAsync(string codigoUsuario, TimeSpan cacheTtl, string jti)
    {
        var cacheKey = $"user-identity:{codigoUsuario}:{jti}";

        if (_cache.TryGetValue(cacheKey, out UsuarioIdentitySnapshot? snapshot))
            return snapshot;

        var usuario = await _usuarioQueryRepository.GetByCuentaLogueoAsync(codigoUsuario);
        var sistemaTask = await _sucApiClient.IngresarSistemaAsync(codigoUsuario);

        if (usuario == null)
            return null;

        if (sistemaTask == null)
            return null;

        snapshot = new UsuarioIdentitySnapshot
        {
            IdUsuario = usuario.Id,
            IdArea = usuario.IdArea,
            IdAreaPadre = usuario.Area.IdAreaPadre,
            NombreArea = usuario.Area.NombreArea.ToString(),
            DescripcionCortaArea = usuario.Area.DescripcionCorta.ToString(),
            NombreGerencia = usuario.Area.AreaPadre.NombreArea.ToString(),
            IdCargo = usuario.IdCargo,
            Correo = usuario.Email,
            Nombre = usuario.Nombres,
            ApellidoPaterno = usuario.ApellidoPaterno,
            ApellidoMaterno = usuario.ApellidoMaterno,
            NumeroDocumento = usuario.NumeroDocumento,
            CodigoRol = sistemaTask.CodigoRolUsuario,
            NombreRol = sistemaTask.NombreRolUsuario,
            Rol = MapearRol(sistemaTask.CodigoRolUsuario)
        };

        if (cacheTtl > TimeSpan.Zero)
        {
            _cache.Set(cacheKey, snapshot, cacheTtl);
        }

        return snapshot;
    }

    private static RolUsuario MapearRol(string codigo) => codigo switch
    {
        "ADMIN" => RolUsuario.Administrador,
        "COORD" => RolUsuario.Coordinador,
        "SOLAPR" => RolUsuario.SolicitanteAprobador,
        _ => throw new Exception($"Rol desconocido: {codigo}")
    };
}