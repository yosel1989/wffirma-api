namespace COFIDE.WFF.Application.Common.Constants;

/// <summary>
/// Códigos enviados en el campo "mensaje" del NotificacionesHub para que el frontend identifique el tipo de aviso.
/// </summary>
public static class CodigosNotificacionTiempoReal
{
    /// <summary>
    /// Código enviado en el campo <c>mensaje</c> para identificar el aviso de una solicitud por aprobar.
    /// </summary>
    public const string SolicitudPorAprobar = "SOLICITUD_POR_APROBAR";
}
