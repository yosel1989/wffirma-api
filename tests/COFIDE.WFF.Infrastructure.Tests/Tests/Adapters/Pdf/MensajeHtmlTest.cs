using COFIDE.WFF.Infrastructure.Adapters.Services.Pdf.FormatoSolicitud;
using FluentAssertions;

namespace COFIDE.WFF.Infrastructure.Tests.Tests.Adapters.Pdf;

public class MensajeHtmlTest
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("<p><br></p>")]
    public void Mensaje_Vacio_No_Debe_Generar_Bloques(string? html)
    {
        MensajeHtml.Convertir(html).Should().BeEmpty();
    }

    [Fact]
    public void Parrafos_Y_Vinetas_Deben_Conservar_Los_Saltos_De_Linea()
    {
        var bloques = MensajeHtml.Convertir("<p>Primera l&iacute;nea</p><p><strong>Segunda</strong></p><ul><li>uno</li><li>dos</li></ul>");

        bloques.Should().ContainSingle()
            .Which.Should().Be(new TextoMensaje("Primera línea\nSegunda\n• uno\n• dos"));
    }

    [Fact]
    public void Lista_Ordenada_Debe_Numerar_Los_Elementos()
    {
        var bloques = MensajeHtml.Convertir(
            "<ol><li data-list=\"ordered\"><span class=\"ql-ui\"></span>uno</li><li data-list=\"ordered\">dos</li><li data-list=\"bullet\">tres</li></ol>");

        bloques.Should().ContainSingle()
            .Which.Should().Be(new TextoMensaje("1. uno\n2. dos\n• tres"));
    }

    [Fact]
    public void Tabla_Debe_Mantenerse_Como_Tabla_Entre_Los_Textos()
    {
        const string html =
            "<p>Detalle:</p>" +
            "<table><tbody>" +
            "<tr><td><p>Anterior</p></td><td><p>Indice</p></td><td><p>Canal</p></td></tr>" +
            "<tr><td>H2H</td><td>#N/D</td><td>No&nbsp;Ubicado</td></tr>" +
            "</tbody></table>" +
            "<p>Saludos</p>";

        var bloques = MensajeHtml.Convertir(html);

        bloques.Should().HaveCount(3);
        bloques[0].Should().Be(new TextoMensaje("Detalle:"));
        bloques[2].Should().Be(new TextoMensaje("Saludos"));

        var tabla = bloques[1].Should().BeOfType<TablaMensaje>().Subject;
        tabla.Encabezado.Should().BeEmpty();
        tabla.Filas.Should().HaveCount(2);
        tabla.Filas[0].Should().Equal("Anterior", "Indice", "Canal");
        tabla.Filas[1].Should().Equal("H2H", "#N/D", "No Ubicado");
    }

    [Fact]
    public void Tabla_Con_Celdas_De_Encabezado_Debe_Separar_El_Encabezado()
    {
        var bloques = MensajeHtml.Convertir(
            "<table><thead><tr><th>Código</th><th>Monto</th></tr></thead><tbody><tr><td>A-1</td><td>100</td></tr></tbody></table>");

        var tabla = bloques.Should().ContainSingle().Which.Should().BeOfType<TablaMensaje>().Subject;
        tabla.Encabezado.Should().Equal("Código", "Monto");
        tabla.Filas.Should().ContainSingle().Which.Should().Equal("A-1", "100");
    }

    [Fact]
    public void Tabla_Pegada_Desde_Excel_Debe_Ignorar_Estilos_Comentarios_Y_Columnas_Vacias()
    {
        const string html =
            "<html><head><style>td { mso-number-format: General; }</style></head><body>" +
            "<!--StartFragment--><table border=0><col width=80>" +
            "<tr height=20><td class=xl65>Anterior</td><td></td><td>Canal</td></tr>\r\n" +
            "<tr><td>H2H</td><td></td></tr>" +
            "<!--EndFragment--></table></body></html>";

        var tabla = MensajeHtml.Convertir(html).Should().ContainSingle().Which.Should().BeOfType<TablaMensaje>().Subject;

        tabla.Columnas.Should().Be(2);
        tabla.Filas[0].Should().Equal("Anterior", "Canal");
        tabla.Filas[1].Should().Equal("H2H", string.Empty);
    }

    [Fact]
    public void Tabla_Dentro_De_Una_Celda_Debe_Escribirse_Como_Texto_De_Esa_Celda()
    {
        var bloques = MensajeHtml.Convertir("<table><tr><td>x<table><tr><td>a</td><td>b</td></tr></table></td><td>y</td></tr></table>");

        var tabla = bloques.Should().ContainSingle().Which.Should().BeOfType<TablaMensaje>().Subject;
        tabla.Filas.Should().ContainSingle().Which.Should().Equal("x\na b", "y");
    }
}
