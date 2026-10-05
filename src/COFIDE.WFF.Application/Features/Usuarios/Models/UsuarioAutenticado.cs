namespace COFIDE.WFF.Application.Features.Usuarios.Models;

/// <summary>
/// Resultado de la autenticación en el SUC (sistema externo). Es un contrato de integración de Application, no un concepto de negocio.
/// </summary>
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