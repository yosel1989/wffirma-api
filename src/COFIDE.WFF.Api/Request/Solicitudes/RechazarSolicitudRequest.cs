namespace COFIDE.WFF.Api.Request.Solicitudes
{
    public sealed class RechazarSolicitudRequest
    {
        public string MotivoRechazo { get; init; } = default!;
        public string? Comentario { get; init; }
        public List<IFormFile>? Sustentos { get; init; }
    }

}
