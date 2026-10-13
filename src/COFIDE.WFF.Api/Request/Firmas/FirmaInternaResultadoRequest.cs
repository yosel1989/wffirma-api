namespace COFIDE.WFF.Api.Request.Firmas
{
    /// <summary>Resultado de la firma que envía la API de firma digital (callback).</summary>
    public sealed class FirmaInternaResultadoRequest
    {
        public string Code { get; init; } = default!;
        public string? DocumentId { get; init; }
        public string Date { get; init; } = default!;
        public string Signer { get; init; } = default!;
        public string DniSigner { get; init; } = default!;
    }
}
