namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerBandejaEntrada;

public class BandejaEntradaResumenDto
{
    public long Total { get; set; }
    public long Urgente { get; set; }
}

public class BandejaEntradaDto
{
    public long IdSolicitud { get; set; }
    public string? CodigoSolicitud { get; set; }
    public string? TipoSolicitud { get; set; }
    public string? SubTipoSolicitud { get; set; }
    public string? TipoOperacion { get; set; }
    public string? Urgente { get; set; }
    public string? Solicitante { get; set; }
    public DateTime? FechaCreacion { get; set; }
    public DateTime? FechaLimite { get; set; }
    public string AnteriorFirmante { get; set; } = string.Empty;
    public List<string> UltimoFirmante { get; set; } = [];
}

public class BandejaEntradaResponseDto
{
    public long Total { get; set; }
    public long Urgente { get; set; }
    public IEnumerable<BandejaEntradaDto> ListadoSolicitudes { get; set; } = [];
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;
}