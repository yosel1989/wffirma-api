using COFIDE.WFF.Application.Common.Files;
using COFIDE.WFF.Application.Ports.Out;
using Moq;

namespace COFIDE.WFF.Application.Tests.Support;

/// <summary>
/// Dobles de prueba para archivos.
/// Reemplazan a IFormFile (Application ya no depende de ASP.NET) y simulan la composición
/// de rutas que hace FileStorageService, para que las verificaciones sobre rutas sigan siendo reales.
/// </summary>
internal static class ArchivosDePrueba
{
    public static ArchivoEntrada Crear(string nombre = "documento.pdf", byte[]? contenido = null)
    {
        var bytes = contenido ?? [1, 2, 3];
        return new ArchivoEntrada(nombre, "application/pdf", bytes.Length, () => new MemoryStream(bytes));
    }

    /// <summary>Mock de IFileStorageService con la misma composición de rutas que la implementación real.</summary>
    public static Mock<IFileStorageService> Almacenamiento()
    {
        var storage = new Mock<IFileStorageService>();
        storage.Setup(s => s.CombinarRuta(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string carpeta, string nombre) => Path.Combine(carpeta, nombre));
        storage.Setup(s => s.ObtenerCarpetaDocumentos(It.IsAny<string>()))
            .Returns((string codigo) => Path.Combine(codigo, "DOCUMENTOS"));
        storage.Setup(s => s.ObtenerCarpetaSustentos(It.IsAny<string>()))
            .Returns((string codigo) => Path.Combine(codigo, "SUSTENTOS"));
        return storage;
    }
}
