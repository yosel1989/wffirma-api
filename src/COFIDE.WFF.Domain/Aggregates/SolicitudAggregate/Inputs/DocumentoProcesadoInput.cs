namespace COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Inputs;

public sealed class DocumentoProcesadoInput
{
    public string NombreOriginal { get; init; } = default!;
    public string ExtensionOriginal { get; init; } = default!;
    public string RutaOriginal { get; init; } = default!;
    public string HashOriginal { get; init; } = default!;

    public string? NombreCodigo { get; init; }
    public string? ExtensionCodigo { get; init; }
    public string? RutaCodigo { get; init; }
    public string? HashCodigo { get; init; }

    public string? NombreConTag { get; set; }
    public string? ExtensionConTag { get; set; }
    public string? RutaConTag { get; set; }
    public string? HashConTag { get; set; }

    public long PesoDocumento { get; init; }
}
