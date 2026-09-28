namespace COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
public interface INotificacionTiempoRealPort
{
    Task NotificarFirmaDocumentoAsync(
        long solicitudId,
        long solicitudDocumentoId,
        string estadoSolicitud,
        bool firmado,
        string codigoUsuario,
        string mensaje,
        CancellationToken cancellationToken);
}