namespace COFIDE.WFF.Infrastructure.Tests.Support;

/// <summary>Carpeta temporal aislada por prueba; se elimina al terminar.</summary>
internal sealed class DirectorioTemporal : IDisposable
{
    public DirectorioTemporal()
    {
        Ruta = Path.Combine(Path.GetTempPath(), "wff-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Ruta);
    }

    public string Ruta { get; }

    public string Combinar(params string[] partes) => Path.Combine([Ruta, .. partes]);

    /// <summary>Crea un archivo con contenido de texto (y sus carpetas) relativo a la raíz.</summary>
    public string CrearArchivo(string rutaRelativa, string contenido)
    {
        var completa = Combinar(rutaRelativa);
        Directory.CreateDirectory(Path.GetDirectoryName(completa)!);
        File.WriteAllText(completa, contenido);
        return completa;
    }

    public IEnumerable<string> Buscar(string patron) =>
        Directory.EnumerateFiles(Ruta, patron, SearchOption.AllDirectories);

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(Ruta))
                Directory.Delete(Ruta, recursive: true);
        }
        catch (IOException)
        {
            // Algún archivo quedó abierto: el sistema operativo limpiará la carpeta temporal.
        }
    }
}
