namespace COFIDE.WFF.Api.Request.Solicitudes
{
    /// <summary>Códigos de referencia a validar contra otras solicitudes.</summary>
    public sealed class DetectarCodigosReferenciaRequest
    {
        public long? IdSolicitud { get; init; }
        public List<string> CodigosReferencia { get; init; } = new();
    }
}
