using COFIDE.WFF.Application.Features.Maestros.DTOs;
using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Services;
using COFIDE.WFF.Application.Features.Solicitudes.Services;
using COFIDE.WFF.Application.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Inputs;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.DomainServices;
using FluentAssertions;
using Moq;
using Vo = COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Vo;

namespace COFIDE.WFF.Application.Tests.Features.Solicitudes;

public class SolicitudPlantillaServiceTest
{
    [Fact]
    public async Task Crear_Sin_Id_Debe_Armar_Una_Plantilla_No_Maestra()
    {
        var plantillas = new Mock<IPlantillaRepository>();
        var correlativos = CorrelativosNuevos(15);
        plantillas.Setup(p => p.ObtenerAgregadoPlantillaAsync(0, It.IsAny<CancellationToken>(), true))
            .ReturnsAsync((Plantilla?)null);

        var plantilla = await Servicio(plantillas, correlativos).CrearSolicitudPlantilla(
            0,
            """{"nombrePlantilla":"Flujo","descripcion":"Descripcion","etapas":[]}""",
            null,
            10,
            "GTI",
            CancellationToken.None);

        plantilla!.Id.Should().Be(15);
        plantilla.NombrePlantilla.Valor.Should().Be("Flujo");
        plantilla.EsPlantillaMaestra.Should().BeFalse();
    }

    [Fact]
    public async Task Crear_Desde_Maestra_Debe_Copiar_Una_Plantilla_Nueva()
    {
        var origen = Plantilla.Crear(7, "Flujo", "Descripcion", true, 10, "GTI", true);
        var plantillas = new Mock<IPlantillaRepository>();
        plantillas.Setup(p => p.ObtenerAgregadoPlantillaAsync(7, It.IsAny<CancellationToken>(), true))
            .ReturnsAsync(origen);

        var plantilla = await Servicio(plantillas, CorrelativosNuevos(15))
            .CrearSolicitudPlantilla(0, null, 7, 10, "GTI", CancellationToken.None);

        plantilla.Should().NotBeSameAs(origen);
        plantilla!.Id.Should().Be(15);
        plantilla.EsPlantillaMaestra.Should().BeFalse();
        plantilla.NombrePlantilla.Valor.Should().Be("Flujo");
    }

    [Fact]
    public async Task Crear_Con_La_Misma_Plantilla_No_Maestra_Debe_Devolverla()
    {
        var origen = Plantilla.Crear(7, "Flujo", "Descripcion", false, 10, "GTI", false);
        var plantillas = new Mock<IPlantillaRepository>();
        var correlativos = new Mock<IGeneradorCorrelativoIdRepository>();
        plantillas.Setup(p => p.ObtenerAgregadoPlantillaAsync(7, It.IsAny<CancellationToken>(), true))
            .ReturnsAsync(origen);

        var plantilla = await Servicio(plantillas, correlativos)
            .CrearSolicitudPlantilla(7, null, 7, 10, "GTI", CancellationToken.None);

        plantilla.Should().BeSameAs(origen);
        correlativos.Verify(c => c.ObtenerSiguienteIdPlantillaAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CrearFlujo_Debe_Enlazar_La_Plantilla_Con_La_Solicitud()
    {
        var plantilla = Plantilla.Crear(7, "Flujo", "Descripcion", false, 10, "GTI", false);
        var solicitud = Borrador();
        var correlativos = new Mock<IGeneradorCorrelativoIdRepository>();
        var flujo = new Mock<IWorkflowDomainService>();
        correlativos.Setup(c => c.ObtenerSiguienteIdSolicitudPlantillaAsync(It.IsAny<CancellationToken>())).ReturnsAsync(30);
        correlativos.Setup(c => c.ObtenerBloqueSiguienteIdSolitanteFirmanteAsync(0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<long>());
        correlativos.Setup(c => c.ObtenerBloqueSiguienteIdDetalleFirmaAsync(0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<long>());

        await Servicio(correlativos: correlativos, flujo: flujo)
            .CrearFlujoDesdeSolicitud(plantilla, solicitud, CancellationToken.None);

        solicitud.SolicitudPlantilla.Id.Should().Be(30);
        solicitud.SolicitudPlantilla.Plantilla.Should().BeSameAs(plantilla);
        flujo.Verify(f => f.AgregarFirmantesFlujoDesdeEtapas(solicitud, plantilla, It.IsAny<Func<long>>()), Times.Once);
    }

    [Fact]
    public async Task Actualizar_Flujo_Sin_Enlace_Debe_Crear_El_Detalle_De_Firma()
    {
        var plantilla = Plantilla.Crear(7, "Flujo", "Descripcion", false, 10, "GTI", false);
        var etapa = plantilla.AgregarEtapas(3, 1, "Etapa 1", Vo.TipoEtapa.Secuencial, 1, Vo.TipoAprobador.Firmante, 2);
        etapa.AgregarFirmante(9, 100, null, null, null, 100);
        var solicitud = Borrador();
        solicitud.AgregarDocumento(4, "contrato", "pdf", "originales", "hash",
            null, null, null, null, null, null, null, null, 10);
        var enlace = SolicitudPlantilla.Crear(3, solicitud, plantilla);
        enlace.AgregarFirmanteFlujo(11, 9, 3, 1, 100);
        solicitud.AsignarPlantilla(enlace);
        var correlativos = new Mock<IGeneradorCorrelativoIdRepository>();
        correlativos.Setup(c => c.ObtenerBloqueSiguienteIdSolitanteFirmanteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<long> { 20 });
        correlativos.Setup(c => c.ObtenerBloqueSiguienteIdDetalleFirmaAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<long> { 40 });

        await Servicio(correlativos: correlativos).ActualizarFlujoDesdeSolicitud(plantilla, solicitud, CancellationToken.None);

        solicitud.Documentos.Single().DetallesFirma.Should().ContainSingle(d => d.Id == 40 && d.IdSolicitudFirmanteFlujo == 11);
    }

    [Fact]
    public async Task Crear_Sobre_La_Misma_Plantilla_Debe_Conservar_La_Etapa()
    {
        var plantilla = Plantilla.Crear(7, "Flujo", "Descripcion", false, 10, "GTI", false);
        var etapa = plantilla.AgregarEtapas(3, 1, "Etapa 1", Vo.TipoEtapa.Secuencial, 1, Vo.TipoAprobador.Firmante, 2);
        etapa.AgregarFirmante(9, 100, null, null, null, 100);
        var plantillas = new Mock<IPlantillaRepository>();
        var correlativos = new Mock<IGeneradorCorrelativoIdRepository>();
        var parametros = new Mock<IParametroRepository>();
        plantillas.Setup(p => p.ObtenerAgregadoPlantillaAsync(7, It.IsAny<CancellationToken>(), true)).ReturnsAsync(plantilla);
        correlativos.Setup(c => c.ObtenerBloqueSiguienteIdEtapaAsync(0, It.IsAny<CancellationToken>())).ReturnsAsync(new List<long>());
        correlativos.Setup(c => c.ObtenerBloqueSiguienteIdFirmanteAsync(0, It.IsAny<CancellationToken>())).ReturnsAsync(new List<long>());
        parametros.Setup(p => p.ObtenerDetallePorCodigoParametro("TIPO_ETAPA", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ParametroDetalleDto> { new() { Id = 1, Codigo = "TI_ETAPA_SECUENCIAL", Valor = "SECUENCIAL" } });
        parametros.Setup(p => p.ObtenerDetallePorCodigoParametro("TIPO_APROBADOR", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ParametroDetalleDto> { new() { Id = 2, Codigo = "TI_APROBADOR_FIRMANTE", Valor = "FIRMANTE" } });

        var resultado = await new SolicitudPlantillaService(
                plantillas.Object,
                correlativos.Object,
                parametros.Object,
                new SolicitudFlujoDiffService(),
                new Mock<IWorkflowDomainService>().Object)
            .CrearSolicitudPlantilla(7, """
                {"id":7,"nombrePlantilla":"Flujo","descripcion":"Descripcion","etapas":[{"id":3,"ordenEtapa":1,"nombreEtapa":"Etapa 1","idTipoEtapa":1,"idTipoAprobador":2,"firmantes":[{"id":9,"idUsuarioAsignado":100,"idUsuarioOrigen":100}]}]}
                """, 7, 10, "GTI", CancellationToken.None);

        resultado.Should().BeSameAs(plantilla);
        plantilla.Etapas.Should().ContainSingle(e => e.Id == 3 && e.Estado && e.OrdenEtapa == 1);
    }

    [Fact]
    public async Task Crear_Desde_Otra_Plantilla_Debe_Copiar_La_Etapa_Al_Origen()
    {
        var origen = Plantilla.Crear(4, "Origen", "Descripcion", false, 10, "GTI", false);
        var actual = Plantilla.Crear(7, "Flujo", "Descripcion", false, 10, "GTI", false);
        var etapa = actual.AgregarEtapas(3, 1, "Etapa 1", Vo.TipoEtapa.Secuencial, 1, Vo.TipoAprobador.Firmante, 2);
        etapa.AgregarFirmante(9, 100, "Analista", "GTI", "GERENCIA", 100);
        var plantillas = new Mock<IPlantillaRepository>();
        var correlativos = new Mock<IGeneradorCorrelativoIdRepository>();
        var parametros = new Mock<IParametroRepository>();
        plantillas.Setup(p => p.ObtenerAgregadoPlantillaAsync(7, It.IsAny<CancellationToken>(), true)).ReturnsAsync(actual);
        plantillas.Setup(p => p.ObtenerAgregadoPlantillaAsync(4, It.IsAny<CancellationToken>(), true)).ReturnsAsync(origen);
        correlativos.Setup(c => c.ObtenerBloqueSiguienteIdEtapaAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new List<long> { 15 });
        correlativos.Setup(c => c.ObtenerBloqueSiguienteIdFirmanteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new List<long> { 16 });
        parametros.Setup(p => p.ObtenerDetallePorCodigoParametro("TIPO_ETAPA", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ParametroDetalleDto> { new() { Id = 1, Codigo = "TI_ETAPA_SECUENCIAL", Valor = "SECUENCIAL" } });
        parametros.Setup(p => p.ObtenerDetallePorCodigoParametro("TIPO_APROBADOR", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ParametroDetalleDto> { new() { Id = 2, Codigo = "TI_APROBADOR_FIRMANTE", Valor = "FIRMANTE" } });

        var resultado = await new SolicitudPlantillaService(
                plantillas.Object, correlativos.Object, parametros.Object,
                new SolicitudFlujoDiffService(), new Mock<IWorkflowDomainService>().Object)
            .CrearSolicitudPlantilla(4, null, 7, 10, "GTI", CancellationToken.None);

        resultado.Should().BeSameAs(origen);
        origen.Etapas.Should().ContainSingle(e => e.Id == 15 && e.OrdenEtapa == 1 && e.Firmantes.Single().Id == 16);
    }

    [Fact]
    public async Task PlantillaCreacion_Sin_Etapas_Debe_Devolver_La_Plantilla_Base()
    {
        var plantillas = new Mock<IPlantillaRepository>();
        var parametros = Parametros();
        plantillas.Setup(p => p.ObtenerSiguienteIdAsync(It.IsAny<CancellationToken>())).ReturnsAsync(15);
        plantillas.Setup(p => p.ObtenerSiguienteIdEtapaAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        plantillas.Setup(p => p.ObtenerSiguienteIdFirmanteAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var plantilla = await new PlantillaCreacionService(plantillas.Object, parametros.Object)
            .CrearPlantilla(new CrearPlantillaInput
            {
                NombrePlantilla = "Flujo",
                Descripcion = "Descripcion",
                Visibilidad = false,
                EsReutilizable = false,
                Etapas = []
            }, 10, "GTI", CancellationToken.None);

        plantilla.Id.Should().Be(15);
        plantilla.Etapas.Should().BeEmpty();
        plantilla.EsPlantillaMaestra.Should().BeFalse();
    }

    private static SolicitudPlantillaService Servicio(
        Mock<IPlantillaRepository>? plantillas = null,
        Mock<IGeneradorCorrelativoIdRepository>? correlativos = null,
        Mock<IWorkflowDomainService>? flujo = null) =>
        new(
            (plantillas ?? new Mock<IPlantillaRepository>()).Object,
            (correlativos ?? new Mock<IGeneradorCorrelativoIdRepository>()).Object,
            Parametros().Object,
            new Mock<ISolicitudFlujoDiffService>().Object,
            (flujo ?? new Mock<IWorkflowDomainService>()).Object);

    private static Mock<IGeneradorCorrelativoIdRepository> CorrelativosNuevos(long idPlantilla)
    {
        var correlativos = new Mock<IGeneradorCorrelativoIdRepository>();
        correlativos.Setup(c => c.ObtenerSiguienteIdPlantillaAsync(It.IsAny<CancellationToken>())).ReturnsAsync(idPlantilla);
        correlativos.Setup(c => c.ObtenerSiguienteIdEtapaAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        correlativos.Setup(c => c.ObtenerSiguienteIdFirmanteAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return correlativos;
    }

    private static Mock<IParametroRepository> Parametros()
    {
        var parametros = new Mock<IParametroRepository>();
        parametros.Setup(p => p.ObtenerDetallePorCodigoParametro(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ParametroDetalleDto>());
        return parametros;
    }

    private static Solicitud Borrador() =>
        Solicitud.CrearBorrador(8, "GTI-SOL-00000008-2026", 1, 2, 10, 3, 10, 5, 2,
            "Contrato", null, false, false, null, 0);
}
