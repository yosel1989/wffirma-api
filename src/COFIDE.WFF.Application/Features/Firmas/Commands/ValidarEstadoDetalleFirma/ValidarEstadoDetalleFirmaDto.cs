namespace COFIDE.WFF.Application.Features.Firmas.Commands.ValidarEstadoDetalleFirma;

public class ValidarEstadoDetalleFirmaDto
{
    public bool DocumentoBloqueado { get; set; }
    public string? NombreFirmante { get; set; }
    public string? ApellidosFirmante { get; set; }
    public string? Mensaje { get; set; }
}
