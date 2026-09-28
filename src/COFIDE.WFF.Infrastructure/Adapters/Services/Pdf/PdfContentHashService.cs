using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using System.Security.Cryptography;
using System.Text;
using UglyToad.PdfPig;
namespace COFIDE.WFF.Infrastructure.Adapters.Services.Pdf;

public class PdfContentHashService : IPdfContentHashService
{
    public string ObtenerTexto(byte[] pdfBytes)
    {
        using var stream = new MemoryStream(pdfBytes);
        return ObtenerTexto(stream);
    }

    public string ObtenerTexto(Stream pdfStream)
    {
        if (pdfStream.CanSeek)
            pdfStream.Position = 0;

        using var document = PdfDocument.Open(pdfStream);

        var sb = new StringBuilder();

        foreach (var page in document.GetPages())
        {
            sb.AppendLine(page.Text);
        }

        return sb.ToString();
    }


    public string ObtenerHashContenido(byte[] pdfBytes)
    {
        using var stream = new MemoryStream(pdfBytes);
        return ObtenerHashContenido(stream);
    }

    public string ObtenerHashContenido(Stream pdfStream)
    {
        var texto = ObtenerTexto(pdfStream);

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(texto));

        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
