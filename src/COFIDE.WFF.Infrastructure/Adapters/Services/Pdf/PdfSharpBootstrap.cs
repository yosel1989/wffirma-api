using COFIDE.WFF.Infrastructure.Adapters.Services.Pdf.Fonts;
using PdfSharp.Fonts;
namespace COFIDE.WFF.Infrastructure.Adapters.Services.Pdf;

public static class PdfSharpBootstrap
{
    private static bool _initialized = false;

    public static void Configure()
    {
        if (_initialized) return;

        GlobalFontSettings.FontResolver = new CustomFontResolver();

        _initialized = true;
    }
}
