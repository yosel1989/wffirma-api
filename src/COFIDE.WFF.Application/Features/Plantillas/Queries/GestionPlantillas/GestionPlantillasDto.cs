namespace COFIDE.WFF.Application.Features.Plantillas.Queries.GestionPlantillas;

public class GestionPlantillasDto
{
    public long IdPlantilla { get; set; }
    public string? NombrePlantilla { get; set; }
    public string? Creador { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public int NumeroEtapas { get; set; }
    public int NumeroFirmantes { get; set; }
    public int NumeroVisadores { get; set; }
}

public class GestionPlantillasResponseDto
{
    public IEnumerable<GestionPlantillasDto> ListadoPlantillas { get; set; } = [];
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;
}
