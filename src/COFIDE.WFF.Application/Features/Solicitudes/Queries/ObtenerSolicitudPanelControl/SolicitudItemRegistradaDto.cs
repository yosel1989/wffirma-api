namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerSolicitudPanelControl;
public class SolicitudItemRegistradaDto
{
    public long IdSolicitud { get; set; }
    public string? CodigoSolicitud { get; set; }
    public string? TipoSolicitud { get; set; }
    public string? SubTipoOperacion { get; set; }
    public string? TipoOperacion { get; set; }
    public string? Urgente { get; set; }
    public string? FirmanteSiguiente { get; set; }
    public DateTime? FechaRegistro { get; set; }
}

