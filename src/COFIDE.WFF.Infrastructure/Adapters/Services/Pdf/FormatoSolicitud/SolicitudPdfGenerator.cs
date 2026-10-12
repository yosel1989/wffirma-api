using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using COFIDE.WFF.Application.Features.Solicitudes.Models.Pdf;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace COFIDE.WFF.Infrastructure.Adapters.Services.Pdf.FormatoSolicitud;

/// <summary>
/// Formato PDF de una solicitud: datos generales, mensaje, firmantes, adjuntos (solo nombre y tamaño),
/// flujo de aprobaciones, códigos de referencia y destinatarios.
/// </summary>
public sealed class SolicitudPdfGenerator : ISolicitudPdfGenerator
{
    private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-PE");

    public byte[] Generar(SolicitudPdf solicitud)
    {
        PdfSharpBootstrap.Configure();

        using var documento = new PdfDocument();
        documento.Info.Title = $"Solicitud {solicitud.CodigoSolicitud}";

        using (var lienzo = new LienzoPdf(documento))
        {
            Encabezado(lienzo, solicitud);
            DatosGenerales(lienzo, solicitud);
            Mensaje(lienzo, solicitud);
            TablaFirmantes(lienzo, solicitud.Firmantes);
            Archivos(lienzo, "Archivos para firma digital:", solicitud.ArchivosFirma);
            Archivos(lienzo, "Archivos de sustento:", solicitud.ArchivosSustento);
            FlujoAprobaciones(lienzo, solicitud.Etapas);
            CodigosReferencia(lienzo, solicitud.CodigosReferencia);
            Destinatarios(lienzo, solicitud.Destinatarios);
        }

        PiePagina(documento, solicitud.FechaGeneracion);

        using var salida = new MemoryStream();
        documento.Save(salida, false);
        return salida.ToArray();
    }

    private static void Encabezado(LienzoPdf lienzo, SolicitudPdf solicitud)
    {
        var fuente = LienzoPdf.Negrita(15);
        var titulo = $"SOLICITUD: {solicitud.CodigoSolicitud}";
        var alto = LienzoPdf.AltoLinea(fuente);

        if (!solicitud.EsUrgente)
        {
            lienzo.TextoCentrado(titulo, fuente, LienzoPdf.ColorTexto);
        }
        else
        {
            const string urgente = "URGENTE";
            var fuenteUrgente = LienzoPdf.Negrita(10);
            var anchoTitulo = lienzo.Gfx.MeasureString(titulo, fuente).Width;
            var anchoEtiqueta = lienzo.Gfx.MeasureString(urgente, fuenteUrgente).Width + 14;
            var x = lienzo.X0 + ((lienzo.Ancho - anchoTitulo - anchoEtiqueta - 10) / 2);

            lienzo.Gfx.DrawString(titulo, fuente, new XSolidBrush(LienzoPdf.ColorTexto), new XRect(x, lienzo.Y, anchoTitulo, alto), XStringFormats.TopLeft);
            var xEtiqueta = x + anchoTitulo + 10;
            lienzo.Gfx.DrawRoundedRectangle(new XSolidBrush(LienzoPdf.ColorAlerta), xEtiqueta, lienzo.Y + 2, anchoEtiqueta, alto - 4, 6, 6);
            lienzo.Gfx.DrawString(urgente, fuenteUrgente, XBrushes.White,
                new XRect(xEtiqueta, lienzo.Y + 2, anchoEtiqueta, alto - 4), XStringFormats.Center);
            lienzo.Espacio(alto);
        }

        lienzo.Espacio(16);
    }

    private static void DatosGenerales(LienzoPdf lienzo, SolicitudPdf solicitud)
    {
        const double separacion = 10;
        var ancho = (lienzo.Ancho - (separacion * 2)) / 3;

        lienzo.AsegurarEspacio(80);
        var y = lienzo.Y;
        var alto = new[]
        {
            lienzo.Campo("Tipo de solicitud", solicitud.TipoSolicitud, lienzo.X0, ancho, y),
            lienzo.Campo("Sub tipo de solicitud", solicitud.SubTipoSolicitud, lienzo.X0 + ancho + separacion, ancho, y),
            lienzo.Campo("Tipo", solicitud.TipoOperacion, lienzo.X0 + ((ancho + separacion) * 2), ancho, y)
        }.Max();

        lienzo.Y = y + alto + 12;
        lienzo.AsegurarEspacio(40);
        lienzo.Y += lienzo.Campo("Asunto", solicitud.Asunto, lienzo.X0, lienzo.Ancho, lienzo.Y) + 6;
    }

    private static void Mensaje(LienzoPdf lienzo, SolicitudPdf solicitud)
    {
        lienzo.Seccion("Mensaje:");

        var texto = TextoPlano(solicitud.Mensaje);
        lienzo.Parrafo(string.IsNullOrWhiteSpace(texto) ? "Sin mensaje." : texto, LienzoPdf.Normal(10),
            string.IsNullOrWhiteSpace(texto) ? LienzoPdf.ColorSecundario : LienzoPdf.ColorTexto);

        if (solicitud.FechaLimite is { } fechaLimite)
        {
            lienzo.Espacio(4);
            lienzo.Resaltado($"Aprobar hasta el {fechaLimite.ToString("dddd d 'de' MMMM 'de' yyyy", Cultura)}.");
        }

        lienzo.Espacio(6);
    }

    private static void TablaFirmantes(LienzoPdf lienzo, IReadOnlyList<FirmantePdf> firmantes)
    {
        if (firmantes.Count == 0)
            return;

        lienzo.Espacio(4);
        lienzo.Tabla(
            ["Orden", "Rol", "Nombres"],
            [0.6, 2, 3],
            firmantes.Select(f => (IReadOnlyList<string>)new[] { f.Orden.ToString(Cultura), f.Rol, f.Nombres }));
    }

    private static void Archivos(LienzoPdf lienzo, string titulo, IReadOnlyList<ArchivoPdf> archivos)
    {
        lienzo.Seccion(titulo);

        if (archivos.Count == 0)
        {
            lienzo.Parrafo("Sin archivos.", LienzoPdf.Normal(9.5), LienzoPdf.ColorSecundario);
            return;
        }

        lienzo.Tabla(
            ["N°", "Nombre del archivo", "Tamaño"],
            [0.5, 5, 1.2],
            archivos.Select((a, i) => (IReadOnlyList<string>)new[] { (i + 1).ToString(Cultura), a.Nombre, Tamanio(a.PesoBytes) }));
    }

    private static void FlujoAprobaciones(LienzoPdf lienzo, IReadOnlyList<EtapaPdf> etapas)
    {
        lienzo.Seccion("Flujo de aprobaciones:");

        if (etapas.Count == 0)
        {
            lienzo.Parrafo("La solicitud no tiene un flujo de aprobación.", LienzoPdf.Normal(9.5), LienzoPdf.ColorSecundario);
            return;
        }

        var fuenteEtapa = LienzoPdf.Negrita(9.5);
        var fuenteNombre = LienzoPdf.Normal(9.5);
        var fuenteEstado = LienzoPdf.Normal(8.5);
        var alto = LienzoPdf.AltoLinea(fuenteNombre) + 6;
        var anchoBloque = Math.Min(lienzo.Ancho, 330);

        foreach (var etapa in etapas)
        {
            var titulo = $"Etapa {etapa.Orden}: {etapa.TipoEtapa} - {etapa.TipoAprobador}";
            lienzo.AsegurarEspacio((alto * 2) + 4);
            lienzo.Gfx.DrawRectangle(new XSolidBrush(LienzoPdf.ColorFondoEncabezado), lienzo.X0, lienzo.Y, anchoBloque, alto);
            lienzo.Gfx.DrawString(titulo, fuenteEtapa, new XSolidBrush(LienzoPdf.ColorTexto),
                new XRect(lienzo.X0 + 6, lienzo.Y + 3, anchoBloque - 12, alto), XStringFormats.TopLeft);
            lienzo.Espacio(alto);

            foreach (var aprobador in etapa.Aprobadores)
            {
                lienzo.AsegurarEspacio(alto);
                Marcador(lienzo.Gfx, aprobador.Estado, lienzo.X0 + 6, lienzo.Y + (alto / 2));

                lienzo.Gfx.DrawString(aprobador.Nombre, fuenteNombre, new XSolidBrush(LienzoPdf.ColorTexto),
                    new XRect(lienzo.X0 + 22, lienzo.Y + 3, anchoBloque - 130, alto), XStringFormats.TopLeft);

                var (texto, color, fuente) = aprobador.Estado switch
                {
                    EstadoAprobadorPdf.Aprobado => (aprobador.Detalle, LienzoPdf.ColorSecundario, fuenteEstado),
                    EstadoAprobadorPdf.TuTurno => ("Tu turno", LienzoPdf.ColorAlerta, LienzoPdf.Negrita(8.5)),
                    _ => ("Pendiente", LienzoPdf.ColorSecundario, fuenteEstado)
                };
                lienzo.Gfx.DrawString(texto, fuente, new XSolidBrush(color),
                    new XRect(lienzo.X0, lienzo.Y + 3, anchoBloque - 6, alto), XStringFormats.TopRight);

                lienzo.Gfx.DrawLine(new XPen(LienzoPdf.ColorBorde, 0.4), lienzo.X0, lienzo.Y + alto, lienzo.X0 + anchoBloque, lienzo.Y + alto);
                lienzo.Espacio(alto);
            }

            lienzo.Espacio(6);
        }
    }

    /// <summary>Aprobado: círculo verde con check. Tu turno: círculo a medio llenar. Pendiente: círculo vacío.</summary>
    private static void Marcador(XGraphics gfx, EstadoAprobadorPdf estado, double x, double yCentro)
    {
        const double d = 10;
        var y = yCentro - (d / 2);

        switch (estado)
        {
            case EstadoAprobadorPdf.Aprobado:
                gfx.DrawEllipse(new XSolidBrush(LienzoPdf.ColorAprobado), x, y, d, d);
                var pen = new XPen(XColors.White, 1.3);
                gfx.DrawLine(pen, x + 2.5, y + 5.2, x + 4.4, y + 7.1);
                gfx.DrawLine(pen, x + 4.4, y + 7.1, x + 7.6, y + 3.2);
                break;
            case EstadoAprobadorPdf.TuTurno:
                gfx.DrawEllipse(new XPen(LienzoPdf.ColorTexto, 1), XBrushes.White, x, y, d, d);
                gfx.DrawPie(new XSolidBrush(LienzoPdf.ColorTexto), x + 1.5, y + 1.5, d - 3, d - 3, 90, 180);
                break;
            default:
                gfx.DrawEllipse(new XPen(LienzoPdf.ColorSecundario, 1), x, y, d, d);
                break;
        }
    }

    private static void CodigosReferencia(LienzoPdf lienzo, IReadOnlyList<string> codigos)
    {
        lienzo.Seccion("Códigos de referencia:");

        if (codigos.Count == 0)
        {
            lienzo.Parrafo("Sin códigos de referencia.", LienzoPdf.Normal(9.5), LienzoPdf.ColorSecundario);
            return;
        }

        foreach (var codigo in codigos)
            lienzo.Parrafo(codigo, LienzoPdf.Normal(9.5), LienzoPdf.ColorTexto);
    }

    private static void Destinatarios(LienzoPdf lienzo, IReadOnlyList<string> destinatarios)
    {
        lienzo.Seccion("Enviar a:");

        if (destinatarios.Count == 0)
        {
            lienzo.Parrafo("Sin destinatarios adicionales.", LienzoPdf.Normal(9.5), LienzoPdf.ColorSecundario);
            return;
        }

        lienzo.Etiquetas(destinatarios, lienzo.X0);
    }

    private static void PiePagina(PdfDocument documento, DateTime fechaGeneracion)
    {
        var fuente = LienzoPdf.Normal(7.5);
        var brocha = new XSolidBrush(LienzoPdf.ColorSecundario);
        var total = documento.PageCount;

        for (var i = 0; i < total; i++)
        {
            var pagina = documento.Pages[i];
            using var gfx = XGraphics.FromPdfPage(pagina, XGraphicsPdfPageOptions.Append);
            XUnit anchoPagina = pagina.Width;
            XUnit altoPagina = pagina.Height;
            var area = new XRect(42, altoPagina.Point - 34, anchoPagina.Point - 84, 12);

            gfx.DrawString($"Generado el {fechaGeneracion.ToString("dd/MM/yyyy HH:mm", Cultura)}", fuente, brocha, area, XStringFormats.TopLeft);
            gfx.DrawString($"Página {i + 1} de {total}", fuente, brocha, area, XStringFormats.TopRight);
        }
    }

    private static string Tamanio(long? bytes) => bytes switch
    {
        null or <= 0 => "-",
        < 1024 * 1024 => $"{Math.Ceiling(bytes.Value / 1024d).ToString("N0", Cultura)} KB",
        _ => $"{(bytes.Value / 1024d / 1024d).ToString("N1", Cultura)} MB"
    };

    /// <summary>El mensaje se escribe con un editor enriquecido (HTML): se convierte a texto con saltos de línea.</summary>
    private static string TextoPlano(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        var texto = Regex.Replace(html, @"<\s*br\s*/?\s*>", "\n", RegexOptions.IgnoreCase);
        texto = Regex.Replace(texto, @"<\s*/\s*(p|div|li|h[1-6])\s*>", "\n", RegexOptions.IgnoreCase);
        texto = Regex.Replace(texto, @"<\s*li[^>]*>", "• ", RegexOptions.IgnoreCase);
        texto = Regex.Replace(texto, "<[^>]+>", string.Empty);
        texto = WebUtility.HtmlDecode(texto);
        texto = Regex.Replace(texto, @"[ \t]+", " ");
        texto = Regex.Replace(texto, @"\n\s*\n\s*\n+", "\n\n");
        return texto.Trim();
    }
}
