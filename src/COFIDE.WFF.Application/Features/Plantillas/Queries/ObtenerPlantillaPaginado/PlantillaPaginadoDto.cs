namespace COFIDE.WFF.Application.Features.Plantillas.Queries.ObtenerPlantillaPaginado;

public class PlantillaPaginadoDto
{
    public long IdPlantilla { get; init; }
    public string? NombrePlantilla { get; init; }
    public string? Descripcion { get; init; } = null!;
    public long CantidadFirmantes { get; init; }
    public long CantidadVisadores { get; init; }
    public string? GerenciaUsuario { get; init; }
    public string FechaCreado { get; init; } = null!;
    public string FechaModificado { get; init; } = null!;
}