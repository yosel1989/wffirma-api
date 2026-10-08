namespace COFIDE.WFF.Api.Request.Firmas
{
    /// <summary>Documentos de la solicitud a firmar en lote.</summary>
    public sealed class FirmarDocumentosZipRequest
    {
        public List<long> Documentos { get; init; } = new();
    }
}
