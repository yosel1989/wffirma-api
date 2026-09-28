namespace COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Models;
public class FirmanteEditModel
{
    public long Id { get; set; }
    public long IdUsuarioAsignado { get; set; }
    public string? CargoUsuario { get; set; }
    public string? AreaUsuario { get; set; }
    public string? GerenciaUsuario { get; set; }
    public long IdUsuarioOrigen { get; set; }
}
