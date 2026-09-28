namespace COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Inputs;

public class EtapaInput
{
    public long OrdenEtapa { get; set; }
    public string NombreEtapa { get; set; } = null!;
    public long IdTipoEtapa { get; set; }
    public long IdTipoAprobador { get; set; }
    public List<FirmanteInput> Firmantes { get; set; } = default!;
}