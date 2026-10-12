using PdfSharp.Fonts;
namespace COFIDE.WFF.Infrastructure.Adapters.Services.Pdf.Fonts;

public class CustomFontResolver : IFontResolver
{
    public static string DefaultFontName => "Calibri#";

    private const string Regular = "Calibri#";
    private const string Negrita = "Calibri#b";

    public byte[] GetFont(string faceName)
    {
        var archivo = faceName == Negrita ? "calibrib.ttf" : "calibri.ttf";
        var path = Path.Combine(AppContext.BaseDirectory, "Fonts", archivo);

        if (!File.Exists(path))
            throw new FileNotFoundException($"Fuente no encontrada en: {path}");

        return File.ReadAllBytes(path);
    }

    public FontResolverInfo ResolveTypeface(string familyName, bool bold, bool italic)
    {
        // Cualquier familia se resuelve a Calibri. La negrita usa su propio archivo;
        // la cursiva se simula sobre la fuente regular.
        return bold
            ? new FontResolverInfo(Negrita, false, italic)
            : new FontResolverInfo(Regular, false, italic);
    }
}