using System.IO.Compression;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Services;
using COFIDE.WFF.Application.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using FluentAssertions;
using Moq;
using AppEx = COFIDE.WFF.Application.Exceptions;

namespace COFIDE.WFF.Application.Tests.Features.Solicitudes;

public class SolicitudDescargaZipServiceTest
{
    [Fact]
    public async Task Documentos_Sin_Solicitud_Debe_Lanzar_APP_SOL_001()
    {
        var act = () => Servicio(null).DescargarDocumentosZipAsync(8, null, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be("APP_SOL_001");
    }

    [Fact]
    public async Task Documentos_Sin_Activos_Debe_Lanzar_APP_SOL_009()
    {
        var solicitud = Borrador();
        solicitud.AgregarDocumento(4, "contrato", "pdf", "originales", "hash",
            null, null, null, null, null, null, null, null, 10);
        solicitud.Documentos.Single().Estado = false;

        var act = () => Servicio(solicitud).DescargarDocumentosZipAsync(8, null, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be("APP_SOL_009");
    }

    [Fact]
    public async Task Documentos_Debe_Empaquetar_El_Original_Y_Renombrar_El_Duplicado()
    {
        var solicitud = Borrador();
        solicitud.AgregarDocumento(4, "contrato", "pdf", "originales", "hash",
            null, null, null, null, null, null, null, null, 10);
        solicitud.AgregarDocumento(5, "contrato", "pdf", "originales", "hash",
            null, null, null, null, null, null, null, null, 10);
        var archivos = Archivos();

        var (contenido, nombre) = await Servicio(solicitud, archivos)
            .DescargarDocumentosZipAsync(8, null, CancellationToken.None);

        nombre.Should().Be("DOCUMENTOS_GTI-SOL-00000008-2026.zip");
        Entradas(contenido).Should().BeEquivalentTo("DOCUMENTOS/contrato.pdf", "DOCUMENTOS/contrato_2.pdf");
        archivos.Verify(a => a.GetFileAsync(Path.Combine("originales", "contrato.pdf"), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Documentos_Firmados_Deben_Usar_La_Ruta_Firmada()
    {
        var solicitud = Borrador();
        solicitud.AgregarDocumento(4, "contrato", "pdf", "originales", "hash",
            null, null, null, null, null, null, null, null, 10);
        var documento = solicitud.Documentos.Single();
        var detalle = DetalleFirma.Crear(1, 2, 4, null, null, null, null, 10);
        detalle.Firmar("contrato [F]", "pdf", "firmados", "hash-firmado", true);
        documento.AgregarDetalleFirma(detalle);
        var archivos = Archivos();

        await Servicio(solicitud, archivos).DescargarDocumentosZipAsync(8, null, CancellationToken.None);

        archivos.Verify(a => a.GetFileAsync(
            Path.Combine("firmados", "contrato [F].pdf"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Sustentos_Debe_Empaquetar_El_Archivo_Activo()
    {
        var solicitud = Borrador();
        solicitud.AgregarSustento(6, "anexo", "pdf", "sustentos", 10);
        var archivos = Archivos();

        var (contenido, nombre) = await Servicio(solicitud, archivos)
            .DescargarSustentosZipAsync(8, null, CancellationToken.None);

        nombre.Should().Be("SUSTENTOS_GTI-SOL-00000008-2026.zip");
        Entradas(contenido).Should().Equal("SUSTENTOS/anexo.pdf");
        archivos.Verify(a => a.GetFileAsync(Path.Combine("sustentos", "anexo.pdf"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Todo_Sin_Archivos_Debe_Lanzar_APP_SOL_009()
    {
        var act = () => Servicio(Borrador()).DescargarTodoZipAsync(8, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be("APP_SOL_009");
    }

    [Fact]
    public async Task Todo_Debe_Incluir_Documentos_Y_Sustentos()
    {
        var solicitud = Borrador();
        solicitud.AgregarDocumento(4, "contrato", "pdf", "originales", "hash",
            null, null, null, null, null, null, null, null, 10);
        solicitud.AgregarSustento(6, "anexo", "pdf", "sustentos", 10);

        var (contenido, nombre) = await Servicio(solicitud)
            .DescargarTodoZipAsync(8, CancellationToken.None);

        nombre.Should().Be("GTI-SOL-00000008-2026.zip");
        Entradas(contenido).Should().BeEquivalentTo("DOCUMENTOS/contrato.pdf", "SUSTENTOS/anexo.pdf");
    }

    private static SolicitudDescargaZipService Servicio(Solicitud? solicitud, Mock<IFileStorageService>? archivos = null)
    {
        var repositorio = new Mock<ISolicitudRepository>();
        repositorio.Setup(r => r.ObtenerSolicitudConDocumentosYSustentosPorIdAsync(
                8, It.IsAny<List<long>?>(), It.IsAny<List<long>?>(), It.IsAny<CancellationToken>(), false))
            .ReturnsAsync(solicitud);
        return new SolicitudDescargaZipService(repositorio.Object, (archivos ?? Archivos()).Object);
    }

    private static Mock<IFileStorageService> Archivos()
    {
        var archivos = new Mock<IFileStorageService>();
        archivos.Setup(a => a.GetFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new MemoryStream([1, 2, 3]));
        return archivos;
    }

    private static Solicitud Borrador() =>
        Solicitud.CrearBorrador(8, "GTI-SOL-00000008-2026", 1, 2, 10, 3, 10, 5, 2,
            "Contrato", null, false, false, null, 0);

    private static List<string> Entradas(byte[] contenido)
    {
        using var zip = new ZipArchive(new MemoryStream(contenido), ZipArchiveMode.Read);
        return zip.Entries.Select(e => e.FullName).ToList();
    }
}
