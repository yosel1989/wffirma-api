namespace COFIDE.WFF.Application.Features.Firmas.DTOs;

public class ConfiguracionFirmaDto
{
    public int TamanioLetraVisado { get; set; }
    public int TamanioLetraFirmante { get; set; }
    public string NombreTag { get; set; } = default!;
    public string ColorEtiqueta { get; set; } = default!;
    public decimal FirmaMargenAlto { get; set; } = default!;
}
