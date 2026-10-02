namespace COFIDE.WFF.Architecture.Tests.Support;

/// <summary>Acceso al código fuente de la solución para las reglas que no se pueden verificar por reflexión.</summary>
public static class RutasSolucion
{
    private static readonly Lazy<string> _raiz = new(BuscarRaiz);

    public static string Raiz => _raiz.Value;
    public static string Src => Path.Combine(Raiz, "src");

    public static string CarpetaProyecto(string proyecto) => Path.Combine(Src, proyecto);

    public static string Csproj(string proyecto) => Path.Combine(Src, proyecto, $"{proyecto}.csproj");

    /// <summary>Archivos .cs del proyecto indicado (o de todo src), excluyendo bin, obj y migraciones.</summary>
    public static IEnumerable<ArchivoFuente> ArchivosCs(string? proyecto = null)
    {
        var carpeta = proyecto is null ? Src : CarpetaProyecto(proyecto);
        if (!Directory.Exists(carpeta))
            return Enumerable.Empty<ArchivoFuente>();

        return Directory.EnumerateFiles(carpeta, "*.cs", SearchOption.AllDirectories)
            .Where(f => !EnCarpeta(f, "bin") && !EnCarpeta(f, "obj") && !EnCarpeta(f, "Migrations"))
            .Select(f => new ArchivoFuente(f, Path.GetRelativePath(Raiz, f).Replace('\\', '/')))
            .ToList();
    }

    public static IEnumerable<string> ProyectosSrc() =>
        Directory.Exists(Src)
            ? Directory.EnumerateDirectories(Src)
                .Select(d => Path.GetFileName(d))
                .Where(n => !string.IsNullOrEmpty(n) && File.Exists(Csproj(n)))
                .ToList()
            : new List<string>();

    private static bool EnCarpeta(string ruta, string carpeta) =>
        ruta.Replace('\\', '/').Contains($"/{carpeta}/", StringComparison.OrdinalIgnoreCase);

    private static string BuscarRaiz()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (dir.GetFiles("COFIDE.WFF.sln").Length > 0)
                return dir.FullName;
            dir = dir.Parent;
        }
        throw new InvalidOperationException(
            "No se encontró COFIDE.WFF.sln subiendo desde " + AppContext.BaseDirectory);
    }
}

public sealed class ArchivoFuente
{
    private string? _contenido;
    private List<(int Numero, string Texto)>? _lineas;

    public ArchivoFuente(string rutaCompleta, string rutaRelativa)
    {
        RutaCompleta = rutaCompleta;
        RutaRelativa = rutaRelativa;
    }

    public string RutaCompleta { get; }
    public string RutaRelativa { get; }
    public string NombreArchivo => Path.GetFileName(RutaCompleta);
    public string Contenido => _contenido ??= File.ReadAllText(RutaCompleta);

    /// <summary>Líneas de código con su número, sin comentarios (// y /* */) fuera de cadenas.</summary>
    public IReadOnlyList<(int Numero, string Texto)> LineasDeCodigo() => _lineas ??= QuitarComentarios();

    private List<(int Numero, string Texto)> QuitarComentarios()
    {
        var resultado = new List<(int, string)>();
        var enBloque = false;
        var numero = 0;

        foreach (var original in Contenido.Split('\n'))
        {
            numero++;
            var linea = original.TrimEnd('\r');
            var sb = new System.Text.StringBuilder();
            var enCadena = false;

            for (var i = 0; i < linea.Length; i++)
            {
                var c = linea[i];
                var siguiente = i + 1 < linea.Length ? linea[i + 1] : '\0';

                if (enBloque)
                {
                    if (c == '*' && siguiente == '/') { enBloque = false; i++; }
                    continue;
                }

                if (enCadena)
                {
                    sb.Append(c);
                    if (c == '\\') { if (i + 1 < linea.Length) sb.Append(linea[++i]); continue; }
                    if (c == '"') enCadena = false;
                    continue;
                }

                if (c == '/' && siguiente == '/') break;
                if (c == '/' && siguiente == '*') { enBloque = true; i++; continue; }
                if (c == '"') enCadena = true;
                sb.Append(c);
            }

            var texto = sb.ToString();
            if (!string.IsNullOrWhiteSpace(texto))
                resultado.Add((numero, texto));
        }

        return resultado;
    }
}
