using System.Text;
using COFIDE.WFF.Infrastructure.Adapters.Services.FileStorage;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace COFIDE.WFF.Infrastructure.Tests.Support;

/// <summary>Construye los adaptadores de archivos reales apuntando a una carpeta temporal.</summary>
internal static class AlmacenamientoDePrueba
{
    public static FileStorageService Crear(DirectorioTemporal directorio) =>
        new(
            Options.Create(new FileStorageOptions { BaseUrl = directorio.Ruta }),
            NullLogger<FileStorageOptions>.Instance);

    public static ZipStreamingService CrearZip(FileStorageService almacenamiento) =>
        new(almacenamiento, NullLogger<ZipStreamingService>.Instance);

    public static Stream Contenido(string texto) => new MemoryStream(Encoding.UTF8.GetBytes(texto));
}
