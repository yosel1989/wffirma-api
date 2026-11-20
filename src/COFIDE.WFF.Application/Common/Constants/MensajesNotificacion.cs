namespace COFIDE.WFF.Application.Common.Constants;

/// <summary>
/// Mensajes enviados al NotificacionesHub para sus notificaciones en tiempo real.
/// </summary>
public static class MensajesNotificacion
{
    /// <summary>
    /// Texto enviado como mensaje al notificar una solicitud pendiente de firma o aprobación.
    /// </summary>
    public const string SolicitudPorAprobar = "Tiene una solicitud pendiente por firmar o aprobar";
}
