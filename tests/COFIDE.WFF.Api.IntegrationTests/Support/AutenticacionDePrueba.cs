using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace COFIDE.WFF.Api.IntegrationTests.Support;

/// <summary>
/// Reemplaza la validación JWT (cuya clave se obtiene del SUC) por un usuario de prueba con los mismos
/// claims que usa CurrentUserService. Solo autentica si la petición trae la cabecera <see cref="Cabecera"/>.
/// </summary>
public sealed class AutenticacionDePrueba(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder
    ) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string Esquema = "Pruebas";
    public const string Cabecera = "X-Usuario-Prueba";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey(Cabecera))
            return Task.FromResult(AuthenticateResult.NoResult());

        var u = ConfiguracionPruebas.Usuario;
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, u.CodigoUsuario),
            new Claim("UserIdLocal", u.UserIdLocal.ToString()),
            new Claim("UserDni", u.Dni),
            new Claim("UserAreaId", u.AreaId.ToString()),
            new Claim("UserAreaNombre", u.AreaNombre),
            new Claim("UserAreaPadreId", u.AreaPadreId.ToString()),
            new Claim("UserAreaDescripcionCorta", u.AreaDescripcionCorta),
            new Claim("UserCargoId", u.CargoId.ToString()),
            new Claim("UserGerenciaNombre", u.GerenciaNombre),
            new Claim("UserCodigoRolUsuario", u.CodigoRolUsuario),
            new Claim("UserCorreo", u.Correo),
            new Claim("UserNombre", u.Nombre),
            new Claim("UserApellidoPaterno", u.ApellidoPaterno),
            new Claim("UserApellidoMaterno", u.ApellidoMaterno)
        };

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Esquema));
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, Esquema)));
    }
}
