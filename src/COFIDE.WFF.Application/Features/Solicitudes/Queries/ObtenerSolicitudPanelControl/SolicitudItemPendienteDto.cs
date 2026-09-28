namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerSolicitudPanelControl;

public class SolicitudItemPendienteDto
 {
    public long IdSolicitud { get; set; }
    public string? CodigoSolicitud { get; set; }
    public string? Asunto { get; set; }
    public string? Urgente { get; set; }
    public string? Solicitante { get; set; }
    public string? AreaSolicitante { get; set; }
    public List<string> UltimoFirmante { get; set; } = [];
    public DateTime? FechaVencimiento { get; set; }

}

