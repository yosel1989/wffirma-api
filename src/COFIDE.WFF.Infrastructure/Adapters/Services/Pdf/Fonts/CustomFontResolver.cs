using PdfSharp.Fonts;
namespace COFIDE.WFF.Infrastructure.Adapters.Services.Pdf.Fonts;

public class CustomFontResolver : IFontResolver
{
    public static string DefaultFontName => "Calibri#";

    public byte[] GetFont(string faceName)
    {
        var basePath = AppContext.BaseDirectory;

        var path = Path.Combine(basePath, "Fonts", "calibri.ttf");

        if (!File.Exists(path))
            throw new FileNotFoundException($"Fuente no encontrada en: {path}");

        return File.ReadAllBytes(path);
    }

    public FontResolverInfo ResolveTypeface(string familyName, bool bold, bool italic)
    {
        if (familyName.Equals("Calibri", StringComparison.OrdinalIgnoreCase))
        {
            return new FontResolverInfo("Calibri#");
        }
        return new FontResolverInfo("Calibri#");
    }
}