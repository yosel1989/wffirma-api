namespace COFIDE.WFF.Api.Request.Firmas
{
    /// <summary>Detalles de firma a restablecer.</summary>
    public sealed class RestablecerFirmasRequest
    {
        public List<long> IdDetalleFirma { get; init; } = new();
    }
}
