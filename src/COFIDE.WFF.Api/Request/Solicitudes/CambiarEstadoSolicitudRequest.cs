namespace COFIDE.WFF.Api.Request.Solicitudes;

public sealed class CambiarEstadoSolicitudRequest
{
    public long IdNuevoEstado { get; init; }

    public string Motivo { get; init; } = string.Empty;
}
