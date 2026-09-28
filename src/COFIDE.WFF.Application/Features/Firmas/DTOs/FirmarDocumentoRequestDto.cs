namespace COFIDE.WFF.Application.Features.Firmas.DTOs;

public class FirmarDocumentoRequestDto
{
    public string CodigoArchivo { get; set; } = default!;
    public Stream ArchivoStream { get; set; } = default!;
    public string? Referencia { get; set; }
    public int? TipoFirma { get; set; }  
    public int? ModoFirma { get; set; }
    public bool? Tsp { get; set; }
    public string? Tags { get; set; }
    public decimal? VisSigX { get; set; }
    public decimal? VisSigY { get; set; }
    public decimal? VisSigWidth { get; set; }
    public decimal? VisSigHeight { get; set; }
    public string? VisSigGraphic { get; set; }
    public string? VisSigText { get; set; }
    public int? VisSigTextSize { get; set; }
    public int? VisSigPage { get; set; }
    public bool? Ltv { get; set; }
    public string? Bg { get; set; }
    public int? VisSigRotation { get; set; }
    public long TiempoEspera { get; set; }
    public bool EsFirmante { get; set; }
    public bool EsVisador { get; set; }
}
