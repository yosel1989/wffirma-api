using COFIDE.WFF.Domain.Common.Enums;

namespace COFIDE.WFF.Application.Features.Usuarios.Models;

public sealed class UsuarioIdentitySnapshot
{
    public long IdUsuario { get; init; }
    public long? IdArea { get; init; }
    public long? IdAreaPadre { get; init; }
    public string? NombreArea { get; init; }
    public string DescripcionCortaArea { get; init; } = null!;
    public string? NombreGerencia { get; init; }
    public long? IdCargo { get; init; }  
    public string Correo { get; init; } = null!;
    public string Nombre { get; init; } = null!;
    public string ApellidoPaterno { get; init; } = null!;
    public string ApellidoMaterno { get; init; } = null!;
    public string NumeroDocumento { get; init; } = null!;
    public string CodigoRol { get; init; } = null!;
    public string NombreRol { get; init; } = null!;
    public RolUsuario Rol { get; set; }

}
