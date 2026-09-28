namespace COFIDE.WFF.Infrastructure.Adapters.ExternalServices.Suc.DTOs;

public sealed class ObtenerParametroDetalleResponseDto
{
    public string CodigoConfiguracion { get; set; } = string.Empty;
    public string NombreConfiguracion { get; set; } = string.Empty;
    public string? ValConf1 { get; set; }
}
