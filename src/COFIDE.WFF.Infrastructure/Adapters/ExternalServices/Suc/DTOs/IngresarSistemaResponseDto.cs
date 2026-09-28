namespace COFIDE.WFF.Infrastructure.Adapters.ExternalServices.Suc.DTOs;
public class IngresarSistemaResponseDto
{
    public required string CodigoSistema { get; init; }
    public required string NombreSistema { get; init; }
    public required string CodigoRolUsuario { get; init; }
    public required string NombreRolUsuario { get; init; }
    public required string NombreUsuario { get; init; }
    public required string CodigoUsuario { get; init; }
    public required  JwtTokenResponseDto JwtToken { get; init; }
    public List<IniciarSesionMenuResponseDto> lstMenu { get; init; } = [];
    public List<IniciarSesionMenuResponseDto> lstSubMenu { get; init; } = [];
}
