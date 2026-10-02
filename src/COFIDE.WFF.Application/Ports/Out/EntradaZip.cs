namespace COFIDE.WFF.Application.Ports.Out;

/// <summary>
/// Archivo que debe incluirse en un ZIP.
/// </summary>
/// <param name="NombreEnZip">Nombre (y carpeta interna) que tendrá dentro del ZIP, p. ej. "DOCUMENTOS/contrato.pdf".</param>
/// <param name="RutaArchivo">Ruta del archivo en el almacenamiento, tal como la devuelve IFileStorageService.</param>
public sealed record EntradaZip(string NombreEnZip, string RutaArchivo);
