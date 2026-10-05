namespace COFIDE.WFF.Api.Request.Firmas
{
    public sealed class FirmaExternaRequest
    {
        public long IdSolicitudDocumento { get; init; }
        public IFormFile Documento { get; init; } = default!;
    }

}
