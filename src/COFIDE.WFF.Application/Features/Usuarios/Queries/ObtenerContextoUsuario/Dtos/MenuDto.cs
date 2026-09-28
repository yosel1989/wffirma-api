namespace COFIDE.WFF.Application.Features.Usuarios.Queries.ObtenerContextoUsuario.Dtos;

public class MenuDto
{
    public int Id { get; init; }
    public int IdMenuPadre { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string? Descripcion { get; init; }
    public string? DescripcionCompleta { get; init; }
    public string? Url { get; init; }
    public string? Icono { get; init; }
}
