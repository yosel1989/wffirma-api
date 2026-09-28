namespace COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Inputs;

public class DocumentoTagInput
{
    public long IdSolicitudDocumento { get; init; } = default!;
    public string NombreDocumento { get; init; } = default!;
    public int NumeroPagina { get; init; } = default!;
    public decimal UbicacionX { get; init; } = default!;
    public decimal UbicacionY { get; init; } = default!;
}
