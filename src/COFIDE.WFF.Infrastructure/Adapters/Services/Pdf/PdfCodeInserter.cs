using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
namespace COFIDE.WFF.Infrastructure.Adapters.Services.Pdf;

public class PdfCodeInserter : IPdfCodigoInserter
{
    private const string DefaultFont = "Calibri";
    private const float DefaultFontSize = 8f;

    public byte[] InsertCodigo(byte[] inputPdfBytes, string codigoDocumento)
    {
        using var inputStream = new MemoryStream(inputPdfBytes);

        var document = PdfReader.Open(inputStream, PdfDocumentOpenMode.Modify);

        var font = new XFont(DefaultFont, DefaultFontSize, XFontStyleEx.Regular);

        foreach (var page in document.Pages)
        {
            using var gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Prepend);

            var rect = new XRect(0, 20, page.Width, 40);

            gfx.DrawString(
                codigoDocumento,
                font,
                XBrushes.Black,
                rect,
                XStringFormats.TopCenter
            );
        }

        using var outputStream = new MemoryStream();
        document.Save(outputStream);

        return outputStream.ToArray();
    }


    public byte[] InsertTag(
      byte[] inputPdfBytes,
      string tag,
      int pagina,
      double x,
      double y,
      string colorFuente,
      decimal firmaMargenAlto,
      CancellationToken cancellationToken = default)
    {
        return InsertTextInternal(
            inputPdfBytes,
            tag,
            pagina - 1,
            x,
            y,
            colorFuente,
            firmaMargenAlto);
    }

    public Task<Stream> InsertTagAsync(
        Stream inputPdf,
        string tag,
        int pagina,
        double x,
        double y,
        string colorFuente,
        decimal firmaMargenAlto,
        CancellationToken cancellationToken = default)
    {
        if (inputPdf.CanSeek)
            inputPdf.Position = 0;

        var result = InsertTextInternal(
            inputPdf,
            tag,
            pagina - 1,
            x,
            y,
            colorFuente,
            firmaMargenAlto);

        return Task.FromResult<Stream>(result);
    }


    private static byte[] InsertTextInternal(
       byte[] inputPdfBytes,
       string text,
       int? pageIndex,
       double x,
       double y,
       string colorHex,
       decimal marginTop)
    {
        using var inputStream = new MemoryStream(inputPdfBytes);
        using var output = InsertTextInternal(
            inputStream,
            text,
            pageIndex,
            x,
            y,
            colorHex,
            marginTop);

        return output.ToArray();
    }

    private static MemoryStream InsertTextInternal(
       Stream inputPdf,
       string text,
       int? pageIndex,
       double x,
       double y,
       string colorHex,
       decimal marginTop)
    {
        var document = PdfReader.Open(inputPdf, PdfDocumentOpenMode.Modify);

        var page = ResolvePage(document, pageIndex);

        var (posX, posY) = CalculatePosition(page, x, y, marginTop);

        using (var gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Prepend))
        {
            var font = new XFont(DefaultFont, DefaultFontSize, XFontStyleEx.Regular);
            var brush = new XSolidBrush(HexToXColor(colorHex));

            gfx.DrawString(text, font, brush, new XPoint(posX, posY));
        }

        var output = new MemoryStream();
        document.Save(output);
        output.Position = 0;

        return output;
    }

    private static PdfPage ResolvePage(PdfDocument document, int? pageIndex)
    {
        if (pageIndex is null)
            return document.Pages[0];

        if (pageIndex < 0 || pageIndex >= document.Pages.Count)
            throw new ArgumentException("Página inválida");

        return document.Pages[pageIndex.Value];
    }

    private static (double x, double y) CalculatePosition(
        PdfPage page,
        double x,
        double y,
        decimal marginTop)
    {
        var posX = x * page.Width.Point + 5;
        var posY = y * page.Height.Point + (double)marginTop;

        return (posX, posY);
    }

    private static XColor HexToXColor(string hex)
    {
        hex = hex.TrimStart('#');

        return XColor.FromArgb(
            Convert.ToInt32(hex[..2], 16),
            Convert.ToInt32(hex.Substring(2, 2), 16),
            Convert.ToInt32(hex.Substring(4, 2), 16));
    }
}
