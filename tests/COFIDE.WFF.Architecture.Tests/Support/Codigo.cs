using System.Text.RegularExpressions;

namespace COFIDE.WFF.Architecture.Tests.Support;

/// <summary>Búsquedas sobre el código fuente (sin comentarios).</summary>
public static class Codigo
{
    /// <summary>"ruta:línea → fragmento" por cada coincidencia del patrón en los archivos indicados.</summary>
    public static IEnumerable<string> Coincidencias(IEnumerable<ArchivoFuente> archivos, Regex patron)
    {
        foreach (var archivo in archivos)
            foreach (var (numero, texto) in archivo.LineasDeCodigo())
            {
                var m = patron.Match(texto);
                if (m.Success)
                    yield return $"{archivo.RutaRelativa}:{numero} → {Recortar(texto.Trim())}";
            }
    }

    public static Regex Patron(string expresion, bool ignorarMayusculas = false) =>
        new(expresion, RegexOptions.Compiled | (ignorarMayusculas ? RegexOptions.IgnoreCase : RegexOptions.None));

    private static string Recortar(string texto) => texto.Length <= 140 ? texto : texto[..137] + "...";
}
