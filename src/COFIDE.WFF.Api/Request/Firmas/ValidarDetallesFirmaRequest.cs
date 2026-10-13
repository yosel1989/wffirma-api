namespace COFIDE.WFF.Api.Request.Firmas
{
    /// <summary>Detalles de firma cuyo estado se quiere validar.</summary>
    public sealed class ValidarDetallesFirmaRequest
    {
        public List<long> Detalles { get; init; } = new();
    }
}
