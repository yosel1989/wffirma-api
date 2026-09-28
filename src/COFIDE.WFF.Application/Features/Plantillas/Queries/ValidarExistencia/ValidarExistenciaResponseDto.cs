namespace COFIDE.WFF.Application.Features.Plantillas.Queries.ValidarExistencia;

public class ValidarExistenciaResponseDto
{
    public bool Existe { get; set; }
    public long IdPlantilla { get; set; }
    public string? Mensaje { get; set; }
    public string? BotonPrimario { get; set; }
    public string? BotonSecundario { get; set; }
}
