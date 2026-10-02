namespace COFIDE.WFF.Api.Request.Solicitudes
{
    public sealed class AprobarSolicitudRequest
    {
        public string? Comentario { get; init; }
        public List<IFormFile>? Sustentos { get; init; }
    }
}
