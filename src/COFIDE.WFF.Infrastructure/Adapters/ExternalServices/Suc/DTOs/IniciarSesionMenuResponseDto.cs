namespace COFIDE.WFF.Infrastructure.Adapters.ExternalServices.Suc.DTOs;

public class IniciarSesionMenuResponseDto
{
    public int Id { get; init; }
    public int IdMenuPadre { get; init; }
    public required string Nombre { get; init; }
    public required string Descripcion { get; init; }
    public required string DescripcionCompleta { get; init; }
    public required string Url { get; init; }        
    public required string Icono { get; init; }
}
