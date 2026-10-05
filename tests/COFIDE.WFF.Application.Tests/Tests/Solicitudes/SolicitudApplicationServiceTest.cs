using COFIDE.WFF.Application.Features.Firmas.Ports.Out;
using COFIDE.WFF.Application.Features.Jobs.Services;
using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.DTOs;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Services;
using COFIDE.WFF.Application.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Models;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Entities;
<<<<<<<< HEAD:tests/COFIDE.WFF.Application.Tests/Tests/Solicitudes/SolicitudApplicationServiceTest.cs
using VoConst = COFIDE.WFF.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Vo = COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Vo;
using AppEx = COFIDE.WFF.Application.Exceptions;
========
using FluentAssertions;
using Moq;
using System.Reflection;
using AppEx = COFIDE.WFF.Application.Exceptions;
using Vo = COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Vo;
using VoConstant = COFIDE.WFF.Domain.ValueObjects;
using COFIDE.WFF.Application.Tests.Support;
using COFIDE.WFF.Application.Common.Files;
>>>>>>>> eb859a5fb546d4a843ae56b393262105489e2da8:tests/COFIDE.WFF.Application.Tests/Tests/Solicitudes/SolicitudApplicationServiceTests.cs

using COFIDE.WFF.Domain.DomainServices;
namespace COFIDE.WFF.Application.Tests.Tests.Solicitudes;

public class SolicitudApplicationServiceTest
{
    [Fact]
    public async Task CrearSolicitudBase_Sin_Tipo_Debe_Usar_NN_En_El_Codigo()
    {
        var anio = DateTime.Now.Year;
        var servicio = Creacion(tipo: null, idSolicitud: 20, correlativo: 7);

        var solicitud = await servicio.CrearSolicitudBase(
            new SolicitudRequestDto(IdTipoSolicitud: 1, IdSubTipoSolicitud: 2, IdtipoOperacion: 3, Asunto: "  Contrato  "),
            "GTI", 10, 5, 2, CancellationToken.None);

        solicitud.Id.Should().Be(20);
        solicitud.CodigoSolicitud.Should().Be($"GTI-NN-00000007-{anio}");
        solicitud.Asunto.Should().Be("Contrato");
        solicitud.IdUsuarioSolicitante.Should().Be(10);
    }

    [Fact]
    public async Task CrearSolicitudBase_Debe_Usar_La_Descripcion_Corta_Del_Tipo()
    {
        var tipo = new TipoSolicitud();
        Asignar(tipo, typeof(TipoSolicitud), "DescripcionCorta", "SOL");
        var servicio = Creacion(tipo, 21, 3);

        var solicitud = await servicio.CrearSolicitudBase(
            new SolicitudRequestDto(IdTipoSolicitud: 4, Asunto: "Alta"),
            "GTI", 10, 5, 2, CancellationToken.None);

        solicitud.CodigoSolicitud.Should().Be($"GTI-SOL-00000003-{DateTime.Now.Year}");
        solicitud.IdTipoSolicitud.Should().Be(4);
    }

    [Fact]
    public async Task Anular_Debe_Pasar_La_Solicitud_A_Anulado_E_Inactivar_Sus_Documentos()
    {
        var solicitud = Solicitud.CrearBorrador(8, "GTI-SOL-00000008-2026", 1, 2, 10, 3, 10, 5, 2,
            "Contrato", null, false, false, null, 0);
        solicitud.AgregarDocumento(4, "contrato", "pdf", "originales", "hash",
            null, null, null, null, null, null, null, null, 10);

        await new SolicitudAnulacionService().AnularSolicitudAsync(solicitud, 10, CancellationToken.None);

        solicitud.EstadoSolicitud.Should().Be(VoConst.EstadoSolicitud.Anulado);
        solicitud.IdUsuarioAccion.Should().Be(10);
        solicitud.Documentos.Single().Estado.Should().BeFalse();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Sustento_Sin_Archivos_Debe_Responder_Vacio(bool nulo)
    {
        var archivos = ArchivosDePrueba.Almacenamiento();

        var resultado = await new SolicitudSustentoService(archivos.Object)
            .CrearDocumentoSustentoInput(nulo ? null : [], "GTI-SOL", CancellationToken.None);

        resultado.Should().BeEmpty();
        archivos.Verify(a => a.SaveFileAsync(
            It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Sustento_Vacio_Debe_Lanzar()
    {
        var act = () => new SolicitudSustentoService(ArchivosDePrueba.Almacenamiento().Object)
            .CrearDocumentoSustentoInput([Archivo("anexo.pdf", [])], "GTI-SOL", CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("Documento vacío");
    }

    [Fact]
    public async Task Sustento_Debe_Guardar_El_Archivo_Y_Separar_Nombre_Y_Extension()
    {
        var storage = ArchivosDePrueba.Almacenamiento();
        storage.Setup(a => a.SaveFileAsync(It.IsAny<Stream>(), "anexo.PDF", It.IsAny<string>(), "anexo.PDF", It.IsAny<CancellationToken>()))
            .ReturnsAsync("guardado");

        var resultado = await new SolicitudSustentoService(storage.Object)
            .CrearDocumentoSustentoInput([Archivo("anexo.PDF", [1, 2, 3])], "GTI-SOL", CancellationToken.None);

        var sustento = resultado.Single();
        sustento.NombreSustento.Should().Be("anexo");
        sustento.ExtensionSustento.Should().Be("PDF");
        sustento.RutaSustento.Should().Be(Path.Combine("GTI-SOL", "SUSTENTOS"));
        sustento.PesoSustento.Should().Be(3);
    }

    [Fact]
    public async Task Documento_Sin_Archivos_Debe_Responder_Vacio()
    {
        var resultado = await Documento().CrearDocumentoInput(null, "GTI-SOL", false, null, CancellationToken.None);

        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task Documento_Vacio_Debe_Lanzar_APP_SOL_017()
    {
        var act = () => Documento().CrearDocumentoInput([Archivo("contrato.pdf", [])], "GTI-SOL", false, null, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be("APP_SOL_017");
    }

    [Fact]
    public async Task Documento_Con_Firma_Debe_Conservar_El_Original_Como_Codigo()
    {
        var storage = ArchivosDePrueba.Almacenamiento();
        var hash = new Mock<IPdfContentHashService>();
        var validador = new Mock<IDocumentoFirmadoValidator>();
        storage.Setup(a => a.SaveFileAsync(It.IsAny<Stream>(), "contrato.pdf", It.IsAny<string>(), "contrato.pdf", It.IsAny<CancellationToken>()))
            .ReturnsAsync("guardado");
        hash.Setup(h => h.ObtenerHashContenido(It.IsAny<byte[]>())).Returns("hash-1");
        validador.Setup(v => v.TieneFirmaAsync(It.IsAny<Stream>())).ReturnsAsync(true);

        var resultado = await Documento(storage, hash, validador)
            .CrearDocumentoInput([Archivo("contrato.pdf", [9])], "GTI-SOL", true, null, CancellationToken.None);

        var documento = resultado.Single();
        documento.NombreOriginal.Should().Be("contrato");
        documento.ExtensionOriginal.Should().Be("pdf");
        documento.HashOriginal.Should().Be("hash-1");
        documento.NombreCodigo.Should().Be("contrato");
        documento.RutaOriginal.Should().Be(Path.Combine("GTI-SOL", "DOCUMENTOS"));
        storage.Verify(a => a.SaveFileAsync(
            It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Plantilla_Sin_Configuracion_Ni_Id_Debe_Responder_Null()
    {
        var plantillas = new Mock<IPlantillaRepository>();
        plantillas.Setup(p => p.ObtenerAgregadoPlantillaAsync(0, It.IsAny<CancellationToken>(), true))
            .ReturnsAsync((Plantilla?)null);

        var resultado = await PlantillaServicio(plantillas)
            .CrearSolicitudPlantilla(null, null, null, 10, "GTI", CancellationToken.None);

        resultado.Should().BeNull();
    }

    [Fact]
    public async Task Plantilla_Con_Configuracion_Invalida_Debe_Lanzar_APP_SOL_011()
    {
        var act = () => PlantillaServicio(new Mock<IPlantillaRepository>())
            .CrearSolicitudPlantilla(null, "null", null, 10, "GTI", CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be("APP_SOL_011");
    }

    [Fact]
    public async Task ActualizarFlujo_Sin_Plantilla_No_Debe_Reservar_Ids()
    {
        var correlativos = new Mock<IGeneradorCorrelativoIdRepository>();
        var solicitud = Solicitud.CrearBorrador(8, "GTI-SOL-00000008-2026", 1, 2, 10, 3, 10, 5, 2,
            "Contrato", null, false, false, null, 0);

        await PlantillaServicio(correlativos: correlativos)
            .ActualizarFlujoDesdeSolicitud(null, solicitud, CancellationToken.None);

        correlativos.Verify(c => c.ObtenerSiguienteIdSolicitudPlantillaAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void Diff_Debe_Detectar_Etapa_Eliminada_Nueva_Movida_Y_Firmante()
    {
        var plantilla = PlantillaConEtapa();
        var servicio = new FlujoDiffDomainService();
        var tempId = Guid.NewGuid();

        var diff = servicio.CalcularDiff(plantilla, new PlantillaEditModel
        {
            EtapasRequest =
            [
                new EtapaEditModel
                {
                    Id = 3,
                    Orden = 5,
                    Nombre = "Etapa 1",
                    Firmantes = [new FirmanteEditModel { IdUsuarioAsignado = 200 }]
                },
                new EtapaEditModel
                {
                    Id = 0,
                    Orden = 2,
                    Nombre = "Nueva",
                    TempId = tempId,
                    Firmantes = [new FirmanteEditModel { IdUsuarioAsignado = 300 }]
                }
            ]
        });

        diff.EtapasMovidas.Should().ContainSingle(e => e.IdEtapa == 3 && e.OrdenAnterior == 1 && e.OrdenNuevo == 5);
        diff.FirmantesEliminados.Should().ContainSingle(f => f.IdUsuario == 100 && f.IdEtapa == 3);
        diff.FirmantesAgregados.Should().Contain(f => f.IdUsuario == 200 && f.IdEtapa == 3);
        diff.EtapasAgregadas.Should().ContainSingle(e => e.Nombre == "Nueva" && e.TempId == tempId);
        diff.FirmantesAgregados.Should().Contain(f => f.IdUsuario == 300 && f.TempIdEtapa == tempId);
    }

    [Fact]
    public async Task Job_Debe_Devolver_La_Cantidad_Actualizada()
    {
        var repo = new Mock<IDetalleFirmaRepository>();
        repo.Setup(r => r.ActualizarEstadosSolicitudProgramadoAsync(It.IsAny<CancellationToken>())).ReturnsAsync(4);

        var cantidad = await new ActualizarEstadoProgramadoService(repo.Object).EjecutarAsync(CancellationToken.None);

        cantidad.Should().Be(4);
    }

    private static SolicitudCreacionService Creacion(TipoSolicitud? tipo, long idSolicitud, long correlativo)
    {
        var maestros = new Mock<IMaestroRepository>();
        var ids = new Mock<IGeneradorCorrelativoIdRepository>();
        maestros.Setup(m => m.GetTipoSolicitudByIdAsync(It.IsAny<long>(), It.IsAny<CancellationToken>(), true))
            .ReturnsAsync(tipo);
        ids.Setup(i => i.ObtenerSiguienteIdSolicitudAsync(It.IsAny<CancellationToken>())).ReturnsAsync(idSolicitud);
        ids.Setup(i => i.ObtenerSiguienteCodigoSolicitudCorrelativoAsync("GTI", It.IsAny<string>(), DateTime.Now.Year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(correlativo);
        return new SolicitudCreacionService(maestros.Object, ids.Object);
    }

    private static SolicitudDocumentoService Documento(
        Mock<IFileStorageService>? storage = null,
        Mock<IPdfContentHashService>? hash = null,
        Mock<IDocumentoFirmadoValidator>? validador = null) =>
        new(
            (storage ?? ArchivosDePrueba.Almacenamiento()).Object,
            new Mock<IPdfCodigoInserter>().Object,
            (hash ?? new Mock<IPdfContentHashService>()).Object,
            (validador ?? new Mock<IDocumentoFirmadoValidator>()).Object,
            new Mock<IParametroRepository>().Object);

    private static SolicitudPlantillaService PlantillaServicio(
        Mock<IPlantillaRepository>? plantillas = null,
        Mock<IGeneradorCorrelativoIdRepository>? correlativos = null) =>
        new(
            (plantillas ?? new Mock<IPlantillaRepository>()).Object,
            (correlativos ?? new Mock<IGeneradorCorrelativoIdRepository>()).Object,
            new Mock<IParametroRepository>().Object,
            new FlujoDiffDomainService(),
            new Mock<COFIDE.WFF.Domain.DomainServices.IWorkflowDomainService>().Object);

    private static Plantilla PlantillaConEtapa()
    {
        var plantilla = Plantilla.Crear(1, "Flujo", "Descripcion", false, 10, "GTI", true);
        var etapa = plantilla.AgregarEtapas(3, 1, "Etapa 1", Vo.TipoEtapa.Secuencial, 1, Vo.TipoAprobador.Firmante, 2);
        etapa.AgregarFirmante(9, 100, null, null, null, 100);
        return plantilla;
    }

    private static ArchivoEntrada Archivo(string nombre, byte[] contenido) =>
        ArchivosDePrueba.Crear(nombre, contenido);

    private static void Asignar(object destino, Type tipo, string propiedad, object valor)
    {
        var campo = tipo.GetField($"<{propiedad}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
        campo!.SetValue(destino, valor);
    }
}
