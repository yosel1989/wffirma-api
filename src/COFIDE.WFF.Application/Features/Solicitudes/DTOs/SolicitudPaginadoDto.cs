namespace COFIDE.WFF.Application.Features.Solicitudes.DTOs;

public class SolicitudPaginadoDto
{
    public long IdSolicitud { get; set; }
    public string? CodigoSolicitud { get; set; }
    public string? TipoSolicitud { get; set; }
    public string? SubTipoSolicitud { get; set; }
    public string? TipoOperacion { get; set; }
    public string? Urgente { get; set; }
    public string? Solicitante { get; set; }
    public string? Estado { get; set; }
    public DateTime? FechaCreacion { get; set; }
    public long AprobadoresPendientes { get; set; }
    public string? AprobadorActual { get; set; }
}
