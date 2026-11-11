namespace COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Models;
public class PlantillaEditModel
{
    public long Id { get; set; }
    public string? NombrePlantilla { get; set; }
    public string? Descripcion { get; set; }
    public bool? Visibilidad { get; set; }
    public List<EtapaEditModel> EtapasRequest { get; init; } = default!;
}
