namespace COFIDE.WFF.Application.Features.Plantillas.Models;

public class FirmanteFlujoRequest
{
    public long? Id { get; set; }
    public long IdUsuarioAsignado { get; set; }
    public string? CargoUsuario { get; set; }
    public string? AreaUsuario { get; set; }
    public string? GerenciaUsuario { get; set; }
    public long IdUsuarioOrigen { get; set; }
}