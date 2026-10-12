using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace COFIDE.WFF.Infrastructure.Adapters.Services.Pdf.FormatoSolicitud;

/// <summary>
/// Ayudante de diagramación sobre PDFsharp: mantiene la posición vertical, crea páginas nuevas
/// cuando no hay espacio y dibuja texto con ajuste de línea, tablas y etiquetas.
/// </summary>
internal sealed class LienzoPdf : IDisposable
{
    public const string Fuente = "Calibri";

    public static readonly XColor ColorTexto = XColor.FromArgb(33, 33, 33);
    public static readonly XColor ColorSecundario = XColor.FromArgb(117, 117, 117);
    public static readonly XColor ColorBorde = XColor.FromArgb(205, 205, 205);
    public static readonly XColor ColorFondoEncabezado = XColor.FromArgb(242, 242, 242);
    public static readonly XColor ColorFondoSuave = XColor.FromArgb(247, 247, 249);
    public static readonly XColor ColorAlerta = XColor.FromArgb(198, 40, 40);
    public static readonly XColor ColorAprobado = XColor.FromArgb(46, 125, 50);
    public static readonly XColor ColorResaltado = XColor.FromArgb(255, 205, 64);

    private const double Margen = 42;
    private const double MargenInferior = 56;

    private readonly PdfDocument _documento;
    private XGraphics? _gfx;
    private PdfPage? _pagina;

    public LienzoPdf(PdfDocument documento)
    {
        _documento = documento;
        NuevaPagina();
    }

    public XGraphics Gfx => _gfx!;
    public double Y { get; set; }
    public double X0 => Margen;
    public double Ancho => AnchoPagina - (Margen * 2);
    private double AnchoPagina { get { XUnit ancho = _pagina!.Width; return ancho.Point; } }
    private double AltoPagina { get { XUnit alto = _pagina!.Height; return alto.Point; } }
    public IReadOnlyList<PdfPage> Paginas => _documento.Pages.Cast<PdfPage>().ToList();

    public static XFont Normal(double tamanio = 9.5) => new(Fuente, tamanio, XFontStyleEx.Regular);
    public static XFont Negrita(double tamanio = 9.5) => new(Fuente, tamanio, XFontStyleEx.Bold);
    public static double AltoLinea(XFont fuente) => fuente.Size * 1.3;

    public void NuevaPagina()
    {
        _gfx?.Dispose();
        _pagina = _documento.AddPage();
        _pagina.Width = XUnit.FromMillimeter(210);
        _pagina.Height = XUnit.FromMillimeter(297);
        _gfx = XGraphics.FromPdfPage(_pagina);
        Y = Margen;
    }

    /// <summary>Si no caben <paramref name="alto"/> puntos en la página actual, pasa a una nueva.</summary>
    public bool AsegurarEspacio(double alto)
    {
        if (Y + alto <= AltoPagina - MargenInferior)
            return false;

        NuevaPagina();
        return true;
    }

    public void Espacio(double alto) => Y += alto;

    public void TextoCentrado(string texto, XFont fuente, XColor color)
    {
        var alto = AltoLinea(fuente);
        AsegurarEspacio(alto);
        Gfx.DrawString(texto, fuente, new XSolidBrush(color), new XRect(X0, Y, Ancho, alto), XStringFormats.TopCenter);
        Y += alto;
    }

    public void Seccion(string titulo)
    {
        var fuente = Negrita(11.5);
        AsegurarEspacio(AltoLinea(fuente) + 30);
        Y += 10;
        Gfx.DrawString(titulo, fuente, new XSolidBrush(ColorTexto), new XRect(X0, Y, Ancho, AltoLinea(fuente)), XStringFormats.TopLeft);
        Y += AltoLinea(fuente) + 4;
    }

    /// <summary>Párrafo con ajuste de línea. Respeta los saltos de línea del texto.</summary>
    public void Parrafo(string texto, XFont fuente, XColor color, double sangria = 0)
    {
        var alto = AltoLinea(fuente);
        foreach (var linea in Ajustar(texto, fuente, Ancho - sangria))
        {
            AsegurarEspacio(alto);
            Gfx.DrawString(linea, fuente, new XSolidBrush(color), new XRect(X0 + sangria, Y, Ancho - sangria, alto), XStringFormats.TopLeft);
            Y += alto;
        }
    }

    /// <summary>Texto en negrita sobre un fondo resaltado.</summary>
    public void Resaltado(string texto)
    {
        var fuente = Negrita(10.5);
        var alto = AltoLinea(fuente) + 4;
        AsegurarEspacio(alto);
        var ancho = Math.Min(Gfx.MeasureString(texto, fuente).Width + 10, Ancho);
        Gfx.DrawRectangle(new XSolidBrush(ColorResaltado), X0, Y, ancho, alto);
        Gfx.DrawString(texto, fuente, new XSolidBrush(ColorTexto), new XRect(X0 + 5, Y + 2, ancho - 10, alto), XStringFormats.TopLeft);
        Y += alto;
    }

    /// <summary>Campo con aspecto de control de formulario: recuadro con la etiqueta sobre el borde.</summary>
    public double Campo(string etiqueta, string valor, double x, double ancho, double y)
    {
        var fuenteEtiqueta = Normal(7);
        var fuenteValor = Normal(9.5);
        var lineas = Ajustar(string.IsNullOrWhiteSpace(valor) ? "-" : valor, fuenteValor, ancho - 16);
        var alto = Math.Max(26, (lineas.Count * AltoLinea(fuenteValor)) + 12);

        Gfx.DrawRoundedRectangle(new XPen(ColorBorde, 0.8), XBrushes.White, x, y, ancho, alto, 6, 6);

        var anchoEtiqueta = Gfx.MeasureString(etiqueta, fuenteEtiqueta).Width + 6;
        Gfx.DrawRectangle(XBrushes.White, x + 8, y - 4, anchoEtiqueta, 8);
        Gfx.DrawString(etiqueta, fuenteEtiqueta, new XSolidBrush(ColorSecundario), new XRect(x + 11, y - 4, anchoEtiqueta, 8), XStringFormats.TopLeft);

        var yTexto = y + 7;
        foreach (var linea in lineas)
        {
            Gfx.DrawString(linea, fuenteValor, new XSolidBrush(ColorTexto), new XRect(x + 8, yTexto, ancho - 16, AltoLinea(fuenteValor)), XStringFormats.TopLeft);
            yTexto += AltoLinea(fuenteValor);
        }

        return alto;
    }

    /// <summary>Tabla con encabezado; ajusta el texto de cada celda y repite el encabezado al cambiar de página.</summary>
    public void Tabla(IReadOnlyList<string> encabezados, IReadOnlyList<double> proporciones, IEnumerable<IReadOnlyList<string>> filas)
    {
        var fuenteEncabezado = Negrita(9);
        var fuenteCelda = Normal(9);
        var total = proporciones.Sum();
        var anchos = proporciones.Select(p => Ancho * p / total).ToArray();
        const double relleno = 5;

        void DibujarEncabezado()
        {
            var alto = AltoLinea(fuenteEncabezado) + (relleno * 2);
            AsegurarEspacio(alto + AltoLinea(fuenteCelda) + (relleno * 2));
            var x = X0;
            for (var i = 0; i < encabezados.Count; i++)
            {
                Gfx.DrawRectangle(new XPen(ColorBorde, 0.6), new XSolidBrush(ColorFondoEncabezado), x, Y, anchos[i], alto);
                Gfx.DrawString(encabezados[i], fuenteEncabezado, new XSolidBrush(ColorTexto),
                    new XRect(x + relleno, Y + relleno, anchos[i] - (relleno * 2), alto), XStringFormats.TopLeft);
                x += anchos[i];
            }
            Y += alto;
        }

        DibujarEncabezado();

        foreach (var fila in filas)
        {
            var lineasPorCelda = fila.Select((celda, i) => Ajustar(celda, fuenteCelda, anchos[i] - (relleno * 2))).ToList();
            var alto = (lineasPorCelda.Max(l => l.Count) * AltoLinea(fuenteCelda)) + (relleno * 2);

            if (AsegurarEspacio(alto))
                DibujarEncabezado();

            var x = X0;
            for (var i = 0; i < anchos.Length; i++)
            {
                Gfx.DrawRectangle(new XPen(ColorBorde, 0.6), XBrushes.White, x, Y, anchos[i], alto);
                var yTexto = Y + relleno;
                foreach (var linea in lineasPorCelda[i])
                {
                    Gfx.DrawString(linea, fuenteCelda, new XSolidBrush(ColorTexto),
                        new XRect(x + relleno, yTexto, anchos[i] - (relleno * 2), AltoLinea(fuenteCelda)), XStringFormats.TopLeft);
                    yTexto += AltoLinea(fuenteCelda);
                }
                x += anchos[i];
            }
            Y += alto;
        }
    }

    /// <summary>Etiquetas (chips) que se acomodan en varias líneas.</summary>
    public void Etiquetas(IEnumerable<string> textos, double xInicial)
    {
        var fuente = Normal(8.5);
        var alto = AltoLinea(fuente) + 6;
        var x = xInicial;
        AsegurarEspacio(alto);

        foreach (var texto in textos)
        {
            var ancho = Math.Min(Gfx.MeasureString(texto, fuente).Width + 16, Ancho);
            if (x + ancho > X0 + Ancho)
            {
                x = xInicial;
                Y += alto + 4;
                AsegurarEspacio(alto);
            }

            Gfx.DrawRoundedRectangle(new XPen(ColorBorde, 0.6), new XSolidBrush(ColorFondoSuave), x, Y, ancho, alto, alto, alto);
            Gfx.DrawString(texto, fuente, new XSolidBrush(ColorTexto), new XRect(x + 8, Y + 3, ancho - 16, alto), XStringFormats.TopLeft);
            x += ancho + 6;
        }

        Y += alto;
    }

    /// <summary>Divide el texto en líneas que caben en el ancho indicado.</summary>
    public List<string> Ajustar(string texto, XFont fuente, double ancho)
    {
        var resultado = new List<string>();
        foreach (var parrafo in (texto ?? string.Empty).Replace("\r", string.Empty).Split('\n'))
        {
            var linea = string.Empty;
            foreach (var palabra in parrafo.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                var candidata = linea.Length == 0 ? palabra : $"{linea} {palabra}";
                if (Gfx.MeasureString(candidata, fuente).Width <= ancho)
                {
                    linea = candidata;
                    continue;
                }

                if (linea.Length > 0)
                    resultado.Add(linea);

                linea = palabra;
                // Palabra más larga que el ancho disponible: se corta por caracteres.
                while (Gfx.MeasureString(linea, fuente).Width > ancho && linea.Length > 1)
                {
                    var corte = linea.Length - 1;
                    while (corte > 1 && Gfx.MeasureString(linea[..corte], fuente).Width > ancho)
                        corte--;
                    resultado.Add(linea[..corte]);
                    linea = linea[corte..];
                }
            }
            resultado.Add(linea);
        }

        return resultado.Count == 0 ? [string.Empty] : resultado;
    }

    public void Dispose()
    {
        _gfx?.Dispose();
        _gfx = null;
    }
}
