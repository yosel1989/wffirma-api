using System.Text.Json.Serialization;

namespace COFIDE.WFF.Application.Features.Firmas.DTOs;

public class FirmaInternaResponseDto
{
    public string? UrlFirma { get; set; }
    public string? CodigoProceso { get; set; }
    public long TiempoEspera { get; set; }

    /*
     * Cuando un visado masivo tenga documentos horizontales y verticales
     */
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<FirmaInternaProcesoDto>? Procesos { get; set; }
}

public class FirmaInternaProcesoDto
{
    public string Orientacion { get; set; } = default!;
    public List<long> Documentos { get; set; } = [];
    public string? UrlFirma { get; set; }
    public string? CodigoProceso { get; set; }
    public long TiempoEspera { get; set; }
}