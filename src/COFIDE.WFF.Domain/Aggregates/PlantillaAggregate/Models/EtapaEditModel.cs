namespace COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Models;

public class EtapaEditModel
{
    public long Id { get; set; }
    public Guid TempId { get; set; }
    public long Orden { get; set; }
    public string Nombre { get; set; } = null!;
    public long IdTipoEtapa { get; set; }
    public long IdTipoAprobador { get; set; }
    public List<FirmanteEditModel> Firmantes { get; set; } = new();
}
