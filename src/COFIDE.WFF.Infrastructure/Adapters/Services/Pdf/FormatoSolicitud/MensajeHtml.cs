using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace COFIDE.WFF.Infrastructure.Adapters.Services.Pdf.FormatoSolicitud;

/// <summary>Parte del mensaje que el PDF dibuja de una sola forma: un texto o una tabla.</summary>
public abstract record BloqueMensaje;

/// <summary>Texto con saltos de línea; las viñetas y los números de lista ya vienen al inicio de cada línea.</summary>
public sealed record TextoMensaje(string Texto) : BloqueMensaje;

/// <summary>
/// Tabla del mensaje, por ejemplo una pegada desde Excel. <see cref="Encabezado"/> viene vacío cuando
/// la tabla no tiene celdas de encabezado (th o thead). Todas las filas tienen la misma cantidad de columnas.
/// </summary>
public sealed record TablaMensaje(IReadOnlyList<string> Encabezado, IReadOnlyList<IReadOnlyList<string>> Filas) : BloqueMensaje
{
    public int Columnas => Encabezado.Count > 0 ? Encabezado.Count : Filas.Count > 0 ? Filas[0].Count : 0;
}

/// <summary>
/// Convierte el HTML del editor enriquecido en bloques para el PDF: el texto conserva párrafos, saltos de línea
/// y viñetas, y las tablas se mantienen como tablas en lugar de mostrar cada celda en una línea.
/// </summary>
public static class MensajeHtml
{
    private static readonly Regex Tokens = new(@"<!--.*?-->|<![^>]*>|<[^>]*>|[^<]+|<", RegexOptions.Singleline | RegexOptions.Compiled);
    private static readonly Regex Etiqueta = new(@"^<\s*(/)?\s*([a-zA-Z][a-zA-Z0-9:-]*)", RegexOptions.Compiled);
    private static readonly Regex TipoLista = new(@"data-list\s*=\s*[""']?([a-zA-Z]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex Espacios = new(@"\s+", RegexOptions.Compiled);
    private static readonly Regex LineasVacias = new(@"\n{3,}", RegexOptions.Compiled);

    /// <summary>Etiquetas cuyo contenido no se muestra (estilos y metadatos que agregan Word o Excel al pegar).</summary>
    private static readonly HashSet<string> SinContenido = new(StringComparer.OrdinalIgnoreCase) { "style", "script", "head", "title", "xml" };

    private static readonly HashSet<string> Bloques = new(StringComparer.OrdinalIgnoreCase)
    {
        "p", "div", "h1", "h2", "h3", "h4", "h5", "h6", "blockquote", "pre"
    };

    public static IReadOnlyList<BloqueMensaje> Convertir(string? html)
    {
        var bloques = new List<BloqueMensaje>();
        if (string.IsNullOrWhiteSpace(html))
            return bloques;

        var texto = new StringBuilder();
        var listas = new Stack<Lista>();
        ConstructorTabla? tabla = null;
        var nivelTabla = 0;
        string? omitiendo = null;

        StringBuilder Destino() => tabla is not null ? tabla.Celda : texto;

        void CerrarTexto()
        {
            var contenido = Normalizar(texto.ToString());
            if (contenido.Length > 0)
                bloques.Add(new TextoMensaje(contenido));
            texto.Clear();
        }

        void CerrarTabla()
        {
            if (tabla?.Construir() is { } construida)
                bloques.Add(construida);
            tabla = null;
            nivelTabla = 0;
        }

        foreach (Match token in Tokens.Matches(html))
        {
            var valor = token.Value;

            if (valor.StartsWith("<!", StringComparison.Ordinal))
                continue;

            var etiqueta = valor.StartsWith('<') ? Etiqueta.Match(valor) : Match.Empty;
            if (!etiqueta.Success)
            {
                if (omitiendo is null)
                    Destino().Append(Decodificar(valor));
                continue;
            }

            var cierre = etiqueta.Groups[1].Success;
            var nombre = etiqueta.Groups[2].Value.ToLowerInvariant();

            if (omitiendo is not null)
            {
                if (cierre && nombre == omitiendo)
                    omitiendo = null;
                continue;
            }

            if (!cierre && SinContenido.Contains(nombre) && !valor.EndsWith("/>", StringComparison.Ordinal))
            {
                omitiendo = nombre;
                continue;
            }

            // Etiquetas con prefijo de Office (o:p, w:sdt...): no aportan contenido propio.
            if (nombre.Contains(':'))
                continue;

            if (nombre == "table")
            {
                if (!cierre)
                {
                    nivelTabla++;
                    if (nivelTabla == 1)
                    {
                        CerrarTexto();
                        tabla = new ConstructorTabla();
                    }
                    else
                    {
                        NuevaLinea(Destino());
                    }
                }
                else if (nivelTabla > 0)
                {
                    nivelTabla--;
                    if (nivelTabla == 0)
                        CerrarTabla();
                    else
                        NuevaLinea(Destino());
                }

                continue;
            }

            if (tabla is not null)
            {
                // Una tabla dentro de una celda se escribe como texto dentro de esa celda.
                if (nivelTabla > 1)
                {
                    if (nombre == "tr" && cierre)
                        NuevaLinea(Destino());
                    else if (nombre is "td" or "th" && cierre)
                        Destino().Append(' ');
                    else if (nombre is "tr" or "td" or "th" or "thead" or "tbody" or "tfoot")
                        continue;
                }
                else
                {
                    switch (nombre)
                    {
                        case "thead":
                            tabla.EnEncabezado = !cierre;
                            continue;
                        case "tbody" or "tfoot" or "caption" or "colgroup" or "col":
                            continue;
                        case "tr":
                            if (cierre) tabla.CerrarFila(); else tabla.AbrirFila();
                            continue;
                        case "td" or "th":
                            if (cierre) tabla.CerrarCelda(); else tabla.AbrirCelda(nombre == "th");
                            continue;
                    }
                }
            }

            switch (nombre)
            {
                case "br":
                    Destino().Append('\n');
                    break;

                case "ul" or "ol":
                    if (cierre)
                    {
                        if (listas.Count > 0)
                            listas.Pop();
                        NuevaLinea(Destino());
                    }
                    else
                    {
                        NuevaLinea(Destino());
                        listas.Push(new Lista(nombre == "ol"));
                    }
                    break;

                case "li":
                    if (cierre)
                    {
                        NuevaLinea(Destino());
                    }
                    else
                    {
                        var destino = Destino();
                        NuevaLinea(destino);
                        destino.Append(Vineta(valor, listas));
                    }
                    break;

                default:
                    if (Bloques.Contains(nombre))
                    {
                        if (cierre)
                            Destino().Append('\n');
                        else
                            NuevaLinea(Destino());
                    }
                    break;
            }
        }

        if (tabla is not null)
            CerrarTabla();
        CerrarTexto();

        return bloques;
    }

    /// <summary>Viñeta o número de la lista. El editor puede indicar el tipo con data-list ("bullet" u "ordered").</summary>
    private static string Vineta(string etiquetaLi, Stack<Lista> listas)
    {
        var lista = listas.Count > 0 ? listas.Peek() : null;
        var tipo = TipoLista.Match(etiquetaLi);
        var ordenada = tipo.Success
            ? tipo.Groups[1].Value.Equals("ordered", StringComparison.OrdinalIgnoreCase)
            : lista?.Ordenada ?? false;

        if (!ordenada)
            return "• ";

        if (lista is null)
            return "1. ";

        lista.Contador++;
        return $"{lista.Contador}. ";
    }

    private static string Decodificar(string texto) => Espacios.Replace(WebUtility.HtmlDecode(texto), " ");

    private static void NuevaLinea(StringBuilder destino)
    {
        if (destino.Length > 0 && destino[^1] != '\n')
            destino.Append('\n');
    }

    /// <summary>Quita los espacios sobrantes de cada línea y deja como máximo una línea en blanco seguida.</summary>
    private static string Normalizar(string texto)
    {
        var lineas = texto.Split('\n').Select(l => Espacios.Replace(l, " ").Trim());
        return LineasVacias.Replace(string.Join('\n', lineas), "\n\n").Trim('\n', ' ');
    }

    private sealed class Lista(bool ordenada)
    {
        public bool Ordenada { get; } = ordenada;
        public int Contador { get; set; }
    }

    private sealed class ConstructorTabla
    {
        private readonly List<(List<string> Celdas, bool EsEncabezado)> _filas = [];
        private readonly StringBuilder _descarte = new();
        private List<string>? _fila;
        private bool _filaSoloEncabezados;
        private StringBuilder? _celda;

        public bool EnEncabezado { get; set; }

        /// <summary>Celda abierta. El texto que queda fuera de una celda (espacios entre etiquetas) se descarta.</summary>
        public StringBuilder Celda
        {
            get
            {
                if (_celda is not null)
                    return _celda;
                _descarte.Clear();
                return _descarte;
            }
        }

        public void AbrirFila()
        {
            CerrarFila();
            _fila = [];
            _filaSoloEncabezados = true;
        }

        public void AbrirCelda(bool esEncabezado)
        {
            CerrarCelda();
            if (_fila is null)
                AbrirFila();
            _celda = new StringBuilder();
            if (!esEncabezado)
                _filaSoloEncabezados = false;
        }

        public void CerrarCelda()
        {
            if (_celda is null)
                return;
            _fila!.Add(Normalizar(_celda.ToString()));
            _celda = null;
        }

        public void CerrarFila()
        {
            CerrarCelda();
            if (_fila is { Count: > 0 })
                _filas.Add((_fila, EnEncabezado || _filaSoloEncabezados));
            _fila = null;
        }

        public TablaMensaje? Construir()
        {
            CerrarFila();

            var filas = _filas.Where(f => f.Celdas.Any(c => c.Length > 0)).ToList();
            if (filas.Count == 0)
                return null;

            // Se descartan las columnas que están vacías en todas las filas (frecuente al pegar desde Excel).
            var totalColumnas = filas.Max(f => f.Celdas.Count);
            var columnas = Enumerable.Range(0, totalColumnas)
                .Where(i => filas.Any(f => i < f.Celdas.Count && f.Celdas[i].Length > 0))
                .ToList();

            IReadOnlyList<string> Completar(List<string> celdas) =>
                columnas.Select(i => i < celdas.Count ? celdas[i] : string.Empty).ToList();

            IReadOnlyList<string> encabezado = filas[0].EsEncabezado ? Completar(filas[0].Celdas) : [];
            var cuerpo = filas.Skip(filas[0].EsEncabezado ? 1 : 0).Select(f => Completar(f.Celdas)).ToList();

            return new TablaMensaje(encabezado, cuerpo);
        }
    }
}
