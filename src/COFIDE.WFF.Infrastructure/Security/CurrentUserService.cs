using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Domain.Common.Enums;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace COFIDE.WFF.Infrastructure.Security;

public class CurrentUserService(
        IHttpContextAccessor httpContextAccessor
    ) : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public bool Autenticado =>
    _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;


    public string? CodigoUsuario
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim is null || string.IsNullOrEmpty(userIdClaim.Value))
                return null;

            return userIdClaim.Value;
        }
    }

    public string? IpAddress
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                ?? context.Connection.RemoteIpAddress?.ToString();

            return ip;
        }
    }

    public long? UserIdLocal
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var claim = user?.FindFirst("UserIdLocal");
            if (claim == null || !long.TryParse(claim.Value, out var id))
                return null;
            return id;
        }
    }

    public string? UserDni
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var claim = user?.FindFirst("UserDni");
            if (claim == null || string.IsNullOrEmpty(claim.Value))
                return null;

            return claim.Value;
        }
    }

    public long? UserAreaId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var claim = user?.FindFirst("UserAreaId");
            if (claim == null || !long.TryParse(claim.Value, out var id))
                return null;

            return id;
        }
    }

    public string? UserAreaNombre
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var claim = user?.FindFirst("UserAreaNombre");
            if (claim == null || string.IsNullOrEmpty(claim.Value))
                return null;

            return claim.Value;
        }
    }

    public long? UserAreaPadreId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var claim = user?.FindFirst("UserAreaPadreId");
            if (claim == null || !long.TryParse(claim.Value, out var id))
                return null;

            return id;
        }
    }

    public string? UserAreaDescripcionCorta
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var claim = user?.FindFirst("UserAreaDescripcionCorta");
            if (claim == null || string.IsNullOrEmpty(claim.Value))
                return null;

            return claim.Value;
        }
    }

    public long? UserCargoId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var claim = user?.FindFirst("UserCargoId");
            if (claim == null || !long.TryParse(claim.Value, out var id))
                return null;

            return id;
        }
    }

    public string? UserGerenciaNombre
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var claim = user?.FindFirst("UserGerenciaNombre");
            if (claim == null || string.IsNullOrEmpty(claim.Value))
                return null;

            return claim.Value;
        }
    }

    public string? UserCodigoRolUsuario
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var claim = user?.FindFirst("UserCodigoRolUsuario");
            if (claim == null || string.IsNullOrEmpty(claim.Value))
                return null;

            return claim.Value;
        }
    }

    public string? UserCorreo
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var claim = user?.FindFirst("UserCorreo");
            if (claim == null || string.IsNullOrEmpty(claim.Value))
                return null;

            return claim.Value;
        }
    }

    public string? NombreUsuario
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var claim = user?.FindFirst("UserNombre");
            if (claim == null || string.IsNullOrEmpty(claim.Value))
                return null;

            return claim.Value;
        }
    }
    public string? ApellidoPaterno
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var claim = user?.FindFirst("UserApellidoPaterno");
            if (claim == null || string.IsNullOrEmpty(claim.Value))
                return null;

            return claim.Value;
        }
    }

    public string? ApellidoMaterno
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var claim = user?.FindFirst("UserApellidoMaterno");
            if (claim == null || string.IsNullOrEmpty(claim.Value))
                return null;

            return claim.Value;
        }
    }

    public string? CodigoRolUsuario
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var claim = user?.FindFirst("UserCodigoRol");
            if (claim == null || string.IsNullOrEmpty(claim.Value))
                return null;

            return claim.Value;
        }
    }

    public string? NombreRolUsuario
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var claim = user?.FindFirst("UserNombreRol");
            if (claim == null || string.IsNullOrEmpty(claim.Value))
                return null;

            return claim.Value;
        }
    }

    public RolUsuario? RolUsuario
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var claim = user?.FindFirst("UserRol");

            if (claim == null)
                return null;

            if (Enum.TryParse<RolUsuario>(claim.Value, out var rol))
                return rol;

            return null;
        }
    }
}
