using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Maestros.DTOs;
using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.DTOs;
using COFIDE.WFF.Application.Features.Solicitudes.Services;
using COFIDE.WFF.Application.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.DomainServices;
using FluentAssertions;
using Moq;

namespace COFIDE.WFF.Application.Tests.Features.Solicitudes;

public class SolicitudFlujoNoMaestroTest
{
    [Fact]
    public async Task CrearPlantilla_Sin_Etapas_Debe_Reservar_El_Id()
    {
        var correlativos = new Mock<IGeneradorCorrelativoIdRepository>();
        correlativos.Setup(c => c.ObtenerSiguienteIdPlantillaAsync(It.IsAny<CancellationToken>())).ReturnsAsync(15);
        correlativos.Setup(c => c.ObtenerSiguienteIdEtapaAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        correlativos.Setup(c => c.ObtenerSiguienteIdFirmanteAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var plantilla = await Servicio(correlativos).CrearPlantillaDesdeSolicitud(
            new CrearSolicitudPlantilla
            {
                NombrePlantilla = "Flujo",
                Descripcion = "Descripcion",
                Visibilidad = false,
                EsReutilizable = false,
                Etapas = []
            },
            Borrador(),
            CancellationToken.None);

        plantilla.Id.Should().Be(15);
        plantilla.Etapas.Should().BeEmpty();
        plantilla.EsPlantillaMaestra.Should().BeFalse();
    }

    [Fact]
    public async Task CrearFlujo_Debe_Enlazar_La_Plantilla()
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

        await Servicio(correlativos, flujo).CrearFlujoDesdeSolicitud(plantilla, solicitud, CancellationToken.None);

        solicitud.SolicitudPlantilla.Id.Should().Be(30);
        solicitud.SolicitudPlantilla.Plantilla.Should().BeSameAs(plantilla);
        flujo.Verify(f => f.AgregarFirmantesFlujoDesdeEtapas(solicitud, plantilla, It.IsAny<Func<long>>()), Times.Once);
    }

    [Fact]
    public async Task ActualizarFlujo_Debe_Reconstruir_Los_Firmantes()
    {
        var plantilla = Plantilla.Crear(7, "Flujo", "Descripcion", false, 10, "GTI", false);
        var solicitud = Borrador();
        solicitud.AsignarPlantilla(SolicitudPlantilla.Crear(3, solicitud, plantilla));
        var correlativos = new Mock<IGeneradorCorrelativoIdRepository>();
        var flujo = new Mock<IWorkflowDomainService>();
        correlativos.Setup(c => c.ObtenerBloqueSiguienteIdSolitanteFirmanteAsync(0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<long>());
        correlativos.Setup(c => c.ObtenerBloqueSiguienteIdDetalleFirmaAsync(0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<long>());

        await Servicio(correlativos, flujo).ActualizarFlujoDesdeSolicitud(plantilla, solicitud, CancellationToken.None);

        flujo.Verify(f => f.ConstruirFirmanteFlujoEtapa(
            solicitud, plantilla, It.IsAny<Dictionary<long, COFIDE.WFF.Domain.Entities.ParametroDetalle>>()), Times.Once);
        flujo.Verify(f => f.ActualizarSolicitudFirmanteNoMaestroFlujo(
            solicitud.SolicitudPlantilla, plantilla, It.IsAny<Func<long>>(), It.IsAny<Func<long>>(), null), Times.Once);
    }

    private static SolicitudFlujoNoMaestroService Servicio(
        Mock<IGeneradorCorrelativoIdRepository> correlativos,
        Mock<IWorkflowDomainService>? flujo = null)
    {
        var parametros = new Mock<IParametroRepository>();
        parametros.Setup(p => p.ObtenerDetallePorCodigoParametro(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ParametroDetalleDto>());
        var usuario = new Mock<ICurrentUserService>();
        usuario.Setup(u => u.UserIdLocal).Returns(10L);
        usuario.Setup(u => u.UserGerenciaNombre).Returns("GTI");
        return new SolicitudFlujoNoMaestroService(
            usuario.Object,
            parametros.Object,
            (flujo ?? new Mock<IWorkflowDomainService>()).Object,
            new Mock<ISolicitudFlujoDiffService>().Object,
            correlativos.Object);
    }

    private static Solicitud Borrador() =>
        Solicitud.CrearBorrador(8, "GTI-SOL-00000008-2026", 1, 2, 10, 3, 10, 5, 2,
            "Contrato", null, false, false, null, 0);
}
