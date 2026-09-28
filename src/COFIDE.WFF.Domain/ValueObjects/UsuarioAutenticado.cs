namespace COFIDE.WFF.Domain.ValueObjects;

public record UsuarioAutenticado(
 string CodigoSistema,
 string NombreSistema,
 string CodigoUsuario,
 string NombreUsuario,
 string CodigoRolUsuario,
 string NombreRolUsuario,
 string Correo,
 string Jwt,
 IReadOnlyCollection<MenuExternoDto> LstMenu,
 IReadOnlyCollection<MenuExternoDto> LstSubMenu);