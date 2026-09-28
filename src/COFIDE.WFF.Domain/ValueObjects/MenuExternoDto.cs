namespace COFIDE.WFF.Domain.ValueObjects;
public record MenuExternoDto(
    int Id,
    int IdMenuPadre,
    string Nombre,
    string? Descripcion,
    string? DescripcionCompleta,
    string? Url,
    string? Icono
);
