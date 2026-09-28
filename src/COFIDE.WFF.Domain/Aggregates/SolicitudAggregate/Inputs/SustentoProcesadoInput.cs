namespace COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Inputs;

public class SustentoProcesadoInput
{
    public string NombreSustento { get; init; } = default!;
    public string ExtensionSustento { get; init; } = default!;
    public string RutaSustento { get; init; } = default!;
    public long PesoSustento { get; init; }
}
