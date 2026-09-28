using COFIDE.WFF.Application.Features.Usuarios.Ports.Out;
using Microsoft.AspNetCore.Authentication;
using System.Globalization;
using System.Security.Claims;
namespace COFIDE.WFF.Infrastructure.Security;

public class UsuarioClaimsTransformation(
        IUsuarioIdentityResolver identityResolver
    ) : IClaimsTransformation
{
    private readonly IUsuarioIdentityResolver _identityResolver= identityResolver;

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var textInfo = CultureInfo.CurrentCulture.TextInfo;

        string Formatear(string valor) => string.IsNullOrWhiteSpace(valor) ? valor : textInfo.ToTitleCase(valor.ToLower());

        if (!principal.Identity?.IsAuthenticated ?? true)
            return principal;

        var identity = principal.Identity as ClaimsIdentity;
        if (identity == null)
            return principal;

        if (identity.HasClaim(c => c.Type == "UserIdLocal"))
            return principal;

        var codigoUsuario = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(codigoUsuario))
            return principal;

        var expClaim = principal.FindFirst("exp")?.Value;
        if (expClaim == null || !long.TryParse(expClaim, out var expUnix))
            return principal;

        var tokenExpirationUtc = DateTimeOffset
            .FromUnixTimeSeconds(expUnix)
            .UtcDateTime;

        var ttl = tokenExpirationUtc - DateTime.UtcNow;
        if (ttl <= TimeSpan.Zero)
            return principal;

        var jti = principal.FindFirst("jti")?.Value;

        var snapshot = await _identityResolver.ResolveAsync(codigoUsuario, ttl, jti);
        if (snapshot == null)
            return principal;

        identity.AddClaim(new Claim("UserIdLocal", snapshot.IdUsuario.ToString()));
        identity.AddClaim(new Claim("UserDni", snapshot.NumeroDocumento.ToString()));
        identity.AddClaim(new Claim("UserCorreo", snapshot.Correo!.ToString()));
        identity.AddClaim(new Claim("UserNombre", Formatear(snapshot.Nombre!)));
        identity.AddClaim(new Claim("UserApellidoPaterno", Formatear(snapshot.ApellidoPaterno!)));
        identity.AddClaim(new Claim("UserApellidoMaterno", Formatear(snapshot.ApellidoMaterno!)));

        if (snapshot.IdArea.HasValue)
        {
            identity.AddClaim(new Claim("UserAreaId", snapshot.IdArea.Value.ToString()));
            identity.AddClaim(new Claim("UserAreaPadreId", snapshot.IdAreaPadre.Value.ToString()));
            identity.AddClaim(new Claim("UserAreaNombre", snapshot.NombreArea!.ToString()));
            identity.AddClaim(new Claim("UserAreaDescripcionCorta", snapshot.DescripcionCortaArea.ToString()));
            identity.AddClaim(new Claim("UserGerenciaNombre", snapshot.NombreGerencia!.ToString()));
        }

        if (snapshot.IdArea.HasValue)
        {
            identity.AddClaim(new Claim("UserCargoId", snapshot.IdCargo!.Value.ToString()));
        }

        // Datos del SUC
        identity.AddClaim(new Claim("UserCodigoRol", snapshot.CodigoRol!.ToUpper()));
        identity.AddClaim(new Claim("UserNombreRol", snapshot.NombreRol!.ToUpper()));
        identity.AddClaim(new Claim("UserRol", snapshot.Rol.ToString()));

        return principal;
    }
}
