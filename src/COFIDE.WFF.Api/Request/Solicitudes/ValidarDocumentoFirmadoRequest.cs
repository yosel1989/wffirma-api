namespace COFIDE.WFF.Api.Request.Solicitudes
{
    public sealed class ValidarDocumentoFirmadoRequest
    {
        public List<IFormFile> Archivos { get; init; } = new();
    }
}
