namespace COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Models;

public class DocumentoEditModel
{
    public long Id { get; set; }
    public string NombreOriginal { get; set; } = null!;
    public string ExtensionOriginal { get; set; } = null!;
    public string RutaOriginal { get; set; } = null!;
    public string? HashOriginal { get; set; } = null!;
    public string? NombreCodigo { get; set; }
    public string? ExtensionCodigo { get; set; }
    public string? RutaCodigo { get; set; }
    public string? HashCodigo { get; set; }
    public long? PesoDocumento { get; set; }
}