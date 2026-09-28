namespace COFIDE.WFF.Infrastructure.Adapters.ExternalServices.Suc.DTOs;
public class ObtenerDatosUsuarioPorCodigoResponseDto
{
    public string? Id { get; init; }
    public string? IdRol { get; init; }
    public string? IdSistema { get; init; }
    public string? Nombre { get; init; }
    public string? Apellido { get; init; }
    public string? LDAP { get; init; }
    public string? Estado { get; init; }
    public string? Rol { get; init; }
    public string? Correo { get; init; }
}
