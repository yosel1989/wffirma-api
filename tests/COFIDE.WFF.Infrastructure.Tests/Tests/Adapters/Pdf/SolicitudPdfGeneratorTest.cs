using COFIDE.WFF.Application.Features.Solicitudes.Models.Pdf;
using COFIDE.WFF.Infrastructure.Adapters.Services.Pdf.FormatoSolicitud;
using FluentAssertions;
using UglyToad.PdfPig;

namespace COFIDE.WFF.Infrastructure.Tests.Tests.Adapters.Pdf;

/// <summary>Genera el PDF real y lee su texto con PdfPig para comprobar el contenido.</summary>
public class SolicitudPdfGeneratorTest
{
    private readonly SolicitudPdfGenerator _sut = new();

    [Fact]
    public void Debe_Generar_Un_Pdf_Con_Todas_Las_Secciones()
    {
        var texto = Texto(_sut.Generar(Solicitud()));

        texto.Should().Contain("SOLICITUD: SOL-1")
            .And.Contain("URGENTE")
            .And.Contain("Órdenes de compra")
            .And.Contain("Autorización de pago")
            .And.Contain("Mensaje:")
            .And.Contain("Archivos para firma digital:")
            .And.Contain("contrato.pdf")
            .And.Contain("Archivos de sustento:")
            .And.Contain("anexo.xlsx")
            .And.Contain("Flujo de aprobaciones:")
            .And.Contain("Etapa 1: Secuencial - Firmante")
            .And.Contain("Tu turno")
            .And.Contain("Códigos de referencia:")
            .And.Contain("2025-2000-1900037134")
            .And.Contain("Enviar a:")
            .And.Contain("carlos@cofide.com.pe");
    }

    [Fact]
    public void Mensaje_Con_Html_Debe_Mostrarse_Como_Texto()
    {
        var texto = Texto(_sut.Generar(Solicitud() with { Mensaje = "<p>Primera l&iacute;nea</p><p><strong>Segunda</strong></p>" }));

        texto.Should().Contain("Primera línea").And.Contain("Segunda").And.NotContain("<p>");
    }

    [Fact]
    public void Solicitud_No_Urgente_No_Debe_Mostrar_La_Etiqueta()
    {
        Texto(_sut.Generar(Solicitud() with { EsUrgente = false })).Should().NotContain("URGENTE");
    }

    [Fact]
    public void Muchos_Adjuntos_Deben_Continuar_En_Paginas_Nuevas()
    {
        var muchos = Enumerable.Range(1, 120).Select(i => new ArchivoPdf($"documento-{i}.pdf", 1024)).ToList();

        var bytes = _sut.Generar(Solicitud() with { ArchivosFirma = muchos });

        using var pdf = PdfDocument.Open(bytes);
        pdf.NumberOfPages.Should().BeGreaterThan(1);
        string.Join(" ", pdf.GetPages().Select(p => p.Text)).Should().Contain("documento-120.pdf");
    }

    [Fact]
    public void Secciones_Vacias_Deben_Indicarlo()
    {
        var vacia = Solicitud() with
        {
            ArchivosFirma = [],
            ArchivosSustento = [],
            Etapas = [],
            Firmantes = [],
            CodigosReferencia = [],
            Destinatarios = []
        };

        var texto = Texto(_sut.Generar(vacia));

        texto.Should().Contain("Sin archivos").And.Contain("Sin códigos de referencia");
    }

    private static string Texto(byte[] pdf)
    {
        pdf.Should().NotBeEmpty();
        using var documento = PdfDocument.Open(pdf);
        // PdfPig devuelve las palabras sin separar en algunos casos: se compara sin espacios extra.
        return string.Join(" ", documento.GetPages().SelectMany(p => p.GetWords()).Select(w => w.Text));
    }

    private static SolicitudPdf Solicitud() => new(
        CodigoSolicitud: "SOL-1",
        EsUrgente: true,
        TipoSolicitud: "Órdenes de compra",
        SubTipoSolicitud: "Bienes",
        TipoOperacion: "Nuevo",
        Asunto: "Autorización de pago a proveedores de RRHH",
        Mensaje: "Se ha registrado el orden de personas a firmar.",
        FechaLimite: new DateTime(2026, 10, 24),
        Firmantes: [new FirmantePdf(1, "Administrador", "José Pérez")],
        ArchivosFirma: [new ArchivoPdf("contrato.pdf", 229_376)],
        ArchivosSustento: [new ArchivoPdf("anexo.xlsx", 10_240)],
        Etapas:
        [
            new EtapaPdf(1, "Secuencial", "Firmante",
            [
                new AprobadorPdf("Mariana Salazar", EstadoAprobadorPdf.Aprobado, "19/11/2025 16:35"),
                new AprobadorPdf("Samuel Gutiérrez", EstadoAprobadorPdf.TuTurno, "Tu Turno"),
                new AprobadorPdf("Ingrid Beltrán", EstadoAprobadorPdf.Pendiente, "Pendiente")
            ])
        ],
        CodigosReferencia: ["2025-2000-1900037134"],
        Destinatarios: ["carlos@cofide.com.pe"],
        FechaGeneracion: new DateTime(2026, 10, 9, 10, 0, 0));
}
