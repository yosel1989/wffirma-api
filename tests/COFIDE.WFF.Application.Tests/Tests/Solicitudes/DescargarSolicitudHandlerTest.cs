using AutoMapper;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.DescargarSolicitudDocumento;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.DescargarSolicitudDocumentoZip;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.DescargarSolicitudSustento;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.DescargarSolicitudSustentoZip;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.DescargarTodoSolicitudZip;
using COFIDE.WFF.Application.Features.Solicitudes.Services;
using COFIDE.WFF.Application.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using FluentAssertions;
using Moq;
using AppEx = COFIDE.WFF.Application.Exceptions;

using COFIDE.WFF.Application.Tests.Support;
namespace COFIDE.WFF.Application.Tests.Solicitudes;

public class DescargarSolicitudHandlerTest
{
    private readonly Mock<IMapper> _mapper = new();
    private readonly Mock<IFileStorageService> _archivos = new();
    private readonly Mock<ISolicitudRepository> _solicitudes = new();
    private readonly byte[] _contenido = [1, 2, 3];

    public DescargarSolicitudHandlerTest()
    {
        _archivos.Setup(a => a.CombinarRuta(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string carpeta, string nombre) => Path.Combine(carpeta, nombre));
        _archivos.Setup(a => a.GetFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new MemoryStream(_contenido));
    }

    [Fact]
    public async Task Documento_Nulo_Debe_Lanzar_APP_SOL_003()
    {
        _solicitudes.Setup(s => s.ObtenerSolicitudDocumentoPorId(4, It.IsAny<CancellationToken>(), false))
            .ReturnsAsync((SolicitudDocumento?)null);

        var act = () => Documento().Handle(new DescargarSolicitudDocumentoQuery(4), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be("APP_SOL_003");
    }

    [Fact]
    public async Task Documento_Original_Debe_Devolver_El_Archivo_En_Base64()
    {
        var documento = PrepararDocumento();
        _solicitudes.Setup(s => s.ObtenerSolicitudDocumentoPorId(4, It.IsAny<CancellationToken>(), false))
            .ReturnsAsync(documento);

        var dto = await Documento().Handle(new DescargarSolicitudDocumentoQuery(4), CancellationToken.None);

        dto.Nombre.Should().Be("contrato");
        dto.Extension.Should().Be("pdf");
        dto.ArchivoBase64.Should().Be(Convert.ToBase64String(_contenido));
        _archivos.Verify(a => a.GetFileAsync(
            Path.Combine("originales", "contrato.pdf"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Documento_Con_Tag_Debe_Tener_Prioridad_Sobre_El_Original()
    {
        var documento = PrepararDocumento(conTag: true);
        _solicitudes.Setup(s => s.ObtenerSolicitudDocumentoPorId(4, It.IsAny<CancellationToken>(), false))
            .ReturnsAsync(documento);

        var dto = await Documento().Handle(new DescargarSolicitudDocumentoQuery(4), CancellationToken.None);

        dto.Nombre.Should().Be("contrato-tag");
        _archivos.Verify(a => a.GetFileAsync(
            Path.Combine("tags", "contrato-tag.pdf"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Documento_Firmado_Debe_Tener_Prioridad_Sobre_El_Con_Tag()
    {
        var documento = PrepararDocumento(conTag: true, firmado: true);
        _solicitudes.Setup(s => s.ObtenerSolicitudDocumentoPorId(4, It.IsAny<CancellationToken>(), false))
            .ReturnsAsync(documento);

        var dto = await Documento().Handle(new DescargarSolicitudDocumentoQuery(4), CancellationToken.None);

        dto.Nombre.Should().Be("contrato-firmado");
        dto.Extension.Should().Be("pdf");
        _archivos.Verify(a => a.GetFileAsync(
            Path.Combine("firmados", "contrato-firmado.pdf"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Documento_Con_Codigo_Debe_Usar_La_Ruta_Codigo()
    {
        var documento = PrepararDocumento(inCodigo: true);
        documento.AsignarDocumentoCodigo("contrato-cod", "pdf", "codigos", "hash-cod");
        _solicitudes.Setup(s => s.ObtenerSolicitudDocumentoPorId(4, It.IsAny<CancellationToken>(), false))
            .ReturnsAsync(documento);

        var dto = await Documento().Handle(new DescargarSolicitudDocumentoQuery(4), CancellationToken.None);

        dto.Nombre.Should().Be("contrato-cod");
        _archivos.Verify(a => a.GetFileAsync(
            Path.Combine("codigos", "contrato-cod.pdf"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Documento_Sin_Ruta_Debe_Lanzar_VALIDATION_FAILED()
    {
        var documento = PrepararDocumento(nombre: "   ");
        _solicitudes.Setup(s => s.ObtenerSolicitudDocumentoPorId(4, It.IsAny<CancellationToken>(), false))
            .ReturnsAsync(documento);

        var act = () => Documento().Handle(new DescargarSolicitudDocumentoQuery(4), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be("VALIDATION_FAILED");
    }

    [Fact]
    public async Task Sustento_Nulo_Debe_Lanzar_APP_SOL_004()
    {
        _solicitudes.Setup(s => s.ObtenerSolicitudSustentoPorId(6, It.IsAny<CancellationToken>(), false))
            .ReturnsAsync((SolicitudSustento?)null);

        var act = () => Sustento().Handle(new DescargarSolicitudSustentoQuery(6), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be("APP_SOL_004");
    }

    [Fact]
    public async Task Sustento_Debe_Devolver_El_Archivo_En_Base64()
    {
        var solicitud = Borrador();
        solicitud.AgregarSustento(6, "anexo", "pdf", "sustentos", 10);
        _solicitudes.Setup(s => s.ObtenerSolicitudSustentoPorId(6, It.IsAny<CancellationToken>(), false))
            .ReturnsAsync(solicitud.Sustentos.Single());

        var dto = await Sustento().Handle(new DescargarSolicitudSustentoQuery(6), CancellationToken.None);

        dto.Nombre.Should().Be("anexo");
        dto.Extension.Should().Be("pdf");
        dto.ArchivoBase64.Should().Be(Convert.ToBase64String(_contenido));
        _archivos.Verify(a => a.GetFileAsync(
            Path.Combine("sustentos", "anexo.pdf"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Zip_De_Documentos_Debe_Delegar_En_El_Servicio()
    {
        var zip = new Mock<ISolicitudDescargaZipService>();
        zip.Setup(z => z.DescargarDocumentosZipAsync(8, It.IsAny<List<long>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new byte[] { 9 }, "documentos.zip"));

        var dto = await new DescargarSolicitudDocumentoZipQueryHandler(_mapper.Object, zip.Object)
            .Handle(new DescargarSolicitudDocumentoZipQuery(8, [4]), CancellationToken.None);

        dto.NombreArchivo.Should().Be("documentos.zip");
        dto.ContentType.Should().Be("application/zip");
        dto.Archivo.Should().Equal(9);
    }

    [Fact]
    public async Task Zip_De_Sustentos_Debe_Delegar_En_El_Servicio()
    {
        var zip = new Mock<ISolicitudDescargaZipService>();
        zip.Setup(z => z.DescargarSustentosZipAsync(8, It.IsAny<List<long>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new byte[] { 8 }, "sustentos.zip"));

        var dto = await new DescargarSolicitudSustentoZipQueryHandler(_mapper.Object, zip.Object)
            .Handle(new DescargarSolicitudSustentoZipQuery(8, [6]), CancellationToken.None);

        dto.NombreArchivo.Should().Be("sustentos.zip");
        dto.ContentType.Should().Be("application/zip");
    }

    [Fact]
    public async Task Zip_Completo_Debe_Delegar_En_El_Servicio()
    {
        var zip = new Mock<ISolicitudDescargaZipService>();
        zip.Setup(z => z.DescargarTodoZipAsync(8, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new byte[] { 7 }, "todo.zip"));

        var dto = await new DescargarTodoSolicitudZipQueryHandler(_mapper.Object, zip.Object)
            .Handle(new DescargarTodoSolicitudZipQuery(8), CancellationToken.None);

        dto.NombreArchivo.Should().Be("todo.zip");
        dto.Archivo.Should().Equal(7);
    }

    private DescargarSolicitudDocumentoHandler Documento() =>
        new(_mapper.Object, _archivos.Object, _solicitudes.Object);

    private DescargarSolicitudSustentoHandler Sustento() =>
        new(_mapper.Object, _archivos.Object, _solicitudes.Object);

    private static SolicitudDocumento PrepararDocumento(
        bool inCodigo = false,
        bool conTag = false,
        bool firmado = false,
        string nombre = "contrato")
    {
        var solicitud = Borrador(inCodigo);
        solicitud.AgregarDocumento(
            4, nombre, "PDF", "originales", "hash-original",
            null, null, null, null, null, null, null, null, 10);
        var documento = solicitud.Documentos.Single();
        NavegacionEf.Asignar(documento, x => x.Solicitud, solicitud);

        if (conTag)
            documento.AsignarDocumentoConTag("contrato-tag", "pdf", "tags", "hash-tag");

        if (firmado)
        {
            var detalle = DetalleFirma.Crear(1, 2, 4, null, null, null, null, 100);
            detalle.Firmar("contrato-firmado", "pdf", "firmados", "hash-firmado", true);
            documento.AgregarDetalleFirma(detalle);
        }

        return documento;
    }

    private static Solicitud Borrador(bool inCodigo = false) =>
        Solicitud.CrearBorrador(8, "GTI-SOL-00000008-2026", 1, 2, 10, 3, 10, 5, 2,
            "Contrato", null, false, inCodigo, null, 0);
}
