namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerSolicitudPanelControl;

public class SolicitudPanelControlDto
{
    public SolicitudPendienteDto SolicitudPendiente { get; set; } = new();
    public long Registradas { get; set; }
    public long Rechazadas { get; set; }
    public List<SolicitudItemPendienteDto> SolicitudItemPendiente { get; set; } = [];
    public List<SolicitudItemRegistradaDto> SolicitudItemRegistrada { get; set; } = [];
}

