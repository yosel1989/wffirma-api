using System.Text.Json;

namespace COFIDE.WFF.Api.Tests.Support;

internal static class JsonResource
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    public static IEnumerable<object[]> Cases<T>(string relativePath) =>
        Load<List<T>>(relativePath).Select(caso => new object[] { caso! });

    private static T Load<T>(string relativePath)
    {
        var ruta = Path.Combine(AppContext.BaseDirectory, "Resources", relativePath);

        if (!File.Exists(ruta))
            throw new FileNotFoundException(
                $"Recurso no copiado al output. Revisa <None Update=\"Resources\\**\\*.json\" " +
                $"CopyToOutputDirectory=\"PreserveNewest\" /> en el csproj. Ruta: {ruta}");

        return JsonSerializer.Deserialize<T>(File.ReadAllText(ruta), Options)
               ?? throw new InvalidOperationException($"JSON vacío o inválido: {ruta}");
    }
}
