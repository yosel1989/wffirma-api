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
