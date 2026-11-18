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

    /// <summary>
    /// Envía al aprobador en turno un aviso de solicitud pendiente de firma, sin documento asociado.
    /// </summary>
    /// <param name="solicitudId">Identificador de la solicitud que origina el aviso.</param>
    /// <param name="totalSolicitudesPendientesFirma">Total de pendientes en turno del destinatario, vistas o no.</param>
    /// <param name="codigoUsuario">Cuenta de logueo del destinatario en el hub.</param>
    /// <param name="mensaje">Código del aviso.</param>
    /// <param name="cancellationToken">Token de cancelación del envío.</param>
    Task NotificarSolicitudPendienteFirma(
        long solicitudId,
        int totalSolicitudesPendientesFirma,
        string codigoUsuario,
        string mensaje,
        CancellationToken cancellationToken);
}