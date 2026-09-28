namespace COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Inputs;

public class CrearPlantillaInput
{
    public string? NombrePlantilla { get; init; }
    public string? Descripcion { get; init; }
    public bool? Visibilidad { get; init; }
    public bool? EsReutilizable { get; init; }
    public List<EtapaInput> Etapas { get; init; } = default!;
}