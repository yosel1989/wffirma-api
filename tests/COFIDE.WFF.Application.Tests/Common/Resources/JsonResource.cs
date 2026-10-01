using System.Text.Json;
namespace COFIDE.WFF.Application.Tests.Common.Resources;

internal static class JsonResource
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true
    };

    /// <summary>
    /// Ruta relativa bajo Resources/, por ejemplo "Solicitudes/correo-electronico.json" 
    /// </summary>
    public static IEnumerable<object[]> Cases<T>(string relativePathUnderResources)
    {
        var ruta = Path.Combine(
        AppContext.BaseDirectory,
        "Resources",
        relativePathUnderResources);

        if (!File.Exists(ruta))
        {
            throw new FileNotFoundException(
        $"No se encontró el JSON en la salida del test. " +
        $"Verifica la carpeta Resources y CopyToOutputDirectory en el .csproj. Ruta: {ruta}");
        }

        var json = File.ReadAllText(ruta);
        var casos = JsonSerializer.Deserialize<List<T>>(json, Options)
        ?? throw new InvalidOperationException($"JSON vacío o inválido: {ruta}");

        return casos.Select(c => new object[] { c! });
    }
}