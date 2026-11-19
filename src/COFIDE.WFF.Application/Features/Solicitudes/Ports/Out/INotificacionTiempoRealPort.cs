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
    /// Envía a los aprobadores en turno un aviso de solicitud pendiente de firma, sin documento asociado.
    /// </summary>
    /// <param name="solicitudId">Identificador de la solicitud que origina el aviso.</param>
    /// <param name="totalSolicitudesPendientesFirma">Total de pendientes en turno compartido por los destinatarios, vistas o no.</param>
    /// <param name="idUsuario">Cuentas de logueo de los destinatarios en el hub.</param>
    /// <param name="mensaje">Texto del mensaje enviado a los destinatarios.</param>
    /// <param name="cancellationToken">Token de cancelación del envío.</param>
    Task NotificarSolicitudPendienteFirma(
        long solicitudId,
        int totalSolicitudesPendientesFirma,
        List<string> idUsuario,
        string mensaje,
        CancellationToken cancellationToken);
}