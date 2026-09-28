namespace COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Inputs;

public class FirmanteInput
{
    public long IdUsuarioAsignado { get; set; }
    public string? CargoUsuario { get; set; }
    public string? AreaUsuario { get; set; }
    public string? GerenciaUsuario { get; set; }
    public long IdUsuarioOrigen { get; set; }
    public long? idSolicitudPlantilla { get; set; }
}
