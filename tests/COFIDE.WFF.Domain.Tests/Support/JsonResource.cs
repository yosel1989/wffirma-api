using System.Text.Json;
namespace COFIDE.WFF.Domain.Tests.Support;

internal static class JsonResource
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    /// <summary>Casos para [MemberData]. Ruta relativa a Resources/.</summary>
    public static IEnumerable<object[]> Cases<T>(string relativePath) =>
        Load<List<T>>(relativePath).Select(caso => new object[] { caso! });

    /// <summary>Un único objeto de configuración.</summary>
    public static T Single<T>(string relativePath) => Load<T>(relativePath);

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