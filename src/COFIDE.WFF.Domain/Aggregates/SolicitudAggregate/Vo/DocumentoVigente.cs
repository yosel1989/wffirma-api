namespace COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Vo;
public sealed record DocumentoVigente(
    string UltimoNombreOriginal,
    string NombreOriginalFirmado,
    string ExtensionOriginal,
    string? Hash,
    string Ruta
);
