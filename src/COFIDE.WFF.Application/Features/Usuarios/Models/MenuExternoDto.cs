namespace COFIDE.WFF.Application.Features.Usuarios.Models;
/// <summary>Opción de menú devuelta por el SUC.</summary>
public record MenuExternoDto(
    int Id,
    int IdMenuPadre,
    string Nombre,
    string? Descripcion,
    string? DescripcionCompleta,
    string? Url,
    string? Icono
);
