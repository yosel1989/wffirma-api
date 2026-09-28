namespace COFIDE.WFF.Application.Features.Usuarios.Queries.UsuarioBuscador;
public class UsuarioBuscadorDto
{
    public long IdUsuario { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string ApellidoPaterno { get; set; } = string.Empty;
    public string ApellidoMaterno { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string Departamento { get; set; } = string.Empty;
    public string Gerencia { get; set; } = string.Empty;
}
