using COFIDE.WFF.Domain.Common.Enums;

namespace COFIDE.WFF.Application.Common.Ports.Out;
public interface ICurrentUserService
{
    bool Autenticado { get; }
    string? CodigoUsuario { get; }
    string? IpAddress { get; }
    long? UserIdLocal { get; }
    string? UserDni { get; }
    long? UserAreaId { get; }
    string? UserAreaNombre { get; }
    long? UserAreaPadreId { get; }
    string? UserAreaDescripcionCorta { get; }
    long? UserCargoId { get; }
    string? UserGerenciaNombre { get; }
    string? UserCodigoRolUsuario { get; }
    string? UserCorreo { get; }
    string? NombreUsuario { get; }
    string? ApellidoPaterno { get; }
    string? ApellidoMaterno { get; }
    string? CodigoRolUsuario { get; }
    string? NombreRolUsuario { get; }
    RolUsuario? RolUsuario { get; }
}
