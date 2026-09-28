namespace COFIDE.WFF.Application.Features.Usuarios.Queries.ObtenerContextoUsuario.Dtos;

public class UsuarioContextoDto
{
    public string CodigoSistema { get; set; } = default!;
    public string NombreSistema { get; set; } = default!;
    public string CodigoUsuario { get; set; } = default!;
    public string NombreUsuario { get; set; } = default!;
    public string CodigoRolUsuario { get; set; } = default!;
    public string NombreRolUsuario { get; set; } = default!;
    public string Correo { get; set; } = default!;
    public long? AreaId { get; set; } = default!;
    public string? NombreArea { get; set; } = default!;
    public long? CargoId { get; set; } = default!;
    public string? NombreCargo { get; set; } = default!;
    public long? GerenciaId { get; set; } = default!;
    public long? DepartamentoId { get; set; } = default!;
    public List<MenuDto> LstMenu { get; set; } = new();
    public List<MenuDto> LstSubMenu { get; set; } = new();
}
