using COFIDE.WFF.Application.Features.Firmas.DTOs;
using COFIDE.WFF.Application.Features.Firmas.Ports.Out;
using PdfSharp.Pdf.IO;

namespace COFIDE.WFF.Infrastructure.Adapters.Services.Pdf;

public class OrientacionPaginaPdfService : IOrientacionPaginaPdfService
{
    public InformacionPaginaPdfDto ObtenerPrimeraPagina(Stream pdfStream)
    {
        if (pdfStream.CanSeek)
            pdfStream.Position = 0;

        using var document = PdfReader.Open(
            pdfStream,
            PdfDocumentOpenMode.ReadOnly);

        if (document.PageCount == 0)
        {
            throw new InvalidOperationException(
                "El documento PDF no contiene páginas.");
        }

        var primeraPagina = document.Pages[0];

        decimal ancho = (decimal)primeraPagina.Width.Point;
        decimal alto = (decimal)primeraPagina.Height.Point;

        var rotacion = primeraPagina.Rotate % 360;

        if (rotacion == 90 || rotacion == 270)
        {
            (ancho, alto) = (alto, ancho);
        }

        return new InformacionPaginaPdfDto(
            Ancho: ancho,
            Alto: alto,
            EsHorizontal: ancho > alto
        );
    }
}