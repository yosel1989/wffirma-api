using COFIDE.WFF.Application.Common.Constants;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Maestros.DTOs;
using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.RestaurarSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Services;
using COFIDE.WFF.Application.Tests.Support;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.DomainServices;
using COFIDE.WFF.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using AppEx = COFIDE.WFF.Application.Exceptions;

namespace COFIDE.WFF.Application.Tests.Solicitudes;

public class SolicitudRestauracionFlujoServiceTest
{
    private readonly Mock<ICurrentUserService> _usuario = new();
    private readonly Mock<ISolicitudRepository> _solicitudes = new();
    private readonly Mock<IPlantillaRepository> _plantillas = new();
    private readonly Mock<IParametroRepository> _parametros = new();
    private readonly Mock<IGeneradorCorrelativoIdRepository> _correlativos = new();
    private readonly Mock<IFileStorageService> _archivos = new();
    private readonly IWorkflowDomainService _workflow = new WorkflowDomainService();

    public SolicitudRestauracionFlujoServiceTest()
    {
        _usuario.Setup(u => u.UserIdLocal).Returns(10);
        _parametros
            .Setup(p => p.ObtenerDetallePorCodigoParametro(CodigosParametro.TipoAprobador, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new ParametroDetalleDto { Id = 10, Codigo = "TI_APROBADOR_FIRMANTE", Valor = "F" }
            ]);
        _correlativos
            .Setup(c => c.ObtenerBloqueSiguienteIdDetalleFirmaAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([900L, 901L, 902L, 903L, 904L]);
        _archivos
            .Setup(a => a.DeleteFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    [Fact]
    public async Task Restaurar_Sin_Usuario_Debe_Lanzar_AUTH_REQUIRED()
    {
        _usuario.Setup(u => u.UserIdLocal).Returns((long?)null);

        var act = () => Servicio().Restaurar(new RestaurarSolicitudCommand(8, 501), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be("AUTH_REQUIRED");
    }

    [Fact]
    public async Task Restaurar_Sin_Solicitud_Debe_Lanzar_APP_SOL_001()
    {
        _solicitudes
            .Setup(r => r.ObtenerSolicitudParaRestauracion(8, It.IsAny<CancellationToken>(), true))
            .ReturnsAsync((Solicitud?)null);

        var act = () => Servicio().Restaurar(new RestaurarSolicitudCommand(8, 501), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be("APP_SOL_001");
    }

    [Fact]
    public async Task Restaurar_Sin_Plantilla_Asignada_Debe_Lanzar_APP_SOL_005()
    {
        var solicitud = Solicitud.CrearBorrador(
            8, "GTI-SOL-00000008-2026", 1, 2, 10, 3, 10, 5, 2,
            "Contrato", null, false, false, null, 0);
        solicitud.Finalizar("Contrato", null, false, false, null, 0, "ana@cofide.com.pe");
        _solicitudes
            .Setup(r => r.ObtenerSolicitudParaRestauracion(8, It.IsAny<CancellationToken>(), true))
            .ReturnsAsync(solicitud);

        var act = () => Servicio().Restaurar(new RestaurarSolicitudCommand(8, 501), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be("APP_SOL_005");
    }

    [Fact]
    public async Task Restaurar_Sin_Plantilla_En_Repositorio_Debe_Lanzar_APP_SOL_005()
    {
        var flujo = RestauracionFlujoFixture.RechazadaEnTercero();
        ConfigurarSolicitudYPlantilla(flujo, plantilla: null);

        var act = () => Servicio().Restaurar(new RestaurarSolicitudCommand(8, flujo.Segundo.Id), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be("APP_SOL_005");
    }

    [Fact]
    public async Task Restaurar_Sin_Ids_De_Detalle_Debe_Lanzar_APP_SOL_006()
    {
        var flujo = RestauracionFlujoFixture.RechazadaEnTercero();
        ConfigurarSolicitudYPlantilla(flujo);
        _correlativos
            .Setup(c => c.ObtenerBloqueSiguienteIdDetalleFirmaAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var act = () => Servicio().Restaurar(new RestaurarSolicitudCommand(8, flujo.Segundo.Id), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be("APP_SOL_006");
    }

    [Fact]
    public async Task Restaurar_Rechazada_Debe_Persistir_El_Flujo_Y_No_Borrar_Archivos()
    {
        var flujo = RestauracionFlujoFixture.RechazadaEnTercero();
        ConfigurarSolicitudYPlantilla(flujo);

        var codigo = await Servicio().Restaurar(new RestaurarSolicitudCommand(8, flujo.Segundo.Id), CancellationToken.None);

        codigo.Should().Be("GTI-SOL-00000008-2026");
        flujo.Solicitud.EstadoSolicitud.Should().Be(EstadoSolicitud.EnCurso);
        flujo.Primero.RevAprobado.Should().BeTrue();
        flujo.Segundo.RevAprobado.Should().BeTrue();
        flujo.Tercero.RevAprobado.Should().BeFalse();
        _archivos.Verify(a => a.DeleteFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _solicitudes.Verify(r => r.GuardarFlujoSolicitudAsync(flujo.Solicitud), Times.Once);
    }

    [Fact]
    public async Task Restaurar_Aprobada_Debe_Borrar_Los_Archivos_Firmados_Desde_El_Punto()
    {
        var flujo = RestauracionFlujoFixture.Aprobada();
        ConfigurarSolicitudYPlantilla(flujo);

        var codigo = await Servicio().Restaurar(new RestaurarSolicitudCommand(8, flujo.Segundo.Id), CancellationToken.None);

        codigo.Should().Be("GTI-SOL-00000008-2026");
        flujo.Solicitud.EstadoSolicitud.Should().Be(EstadoSolicitud.EnCurso);
        flujo.Segundo.RevAprobado.Should().BeFalse();
        flujo.Tercero.RevAprobado.Should().BeFalse();
        _archivos.Verify(a => a.DeleteFileAsync("/firmados/doc-101.pdf", It.IsAny<CancellationToken>()), Times.Once);
        _archivos.Verify(a => a.DeleteFileAsync("/firmados/doc-102.pdf", It.IsAny<CancellationToken>()), Times.Once);
        _solicitudes.Verify(r => r.GuardarFlujoSolicitudAsync(flujo.Solicitud), Times.Once);
    }

    private SolicitudRestauracionFlujoService Servicio() =>
        new(
            _usuario.Object,
            _solicitudes.Object,
            _plantillas.Object,
            _workflow,
            _parametros.Object,
            _correlativos.Object,
            _archivos.Object);

    private void ConfigurarSolicitudYPlantilla(FlujoRestauracionApp flujo, Plantilla? plantilla)
    {
        _solicitudes
            .Setup(r => r.ObtenerSolicitudParaRestauracion(8, It.IsAny<CancellationToken>(), true))
            .ReturnsAsync(flujo.Solicitud);
        _plantillas
            .Setup(r => r.ObtenerAgregadoPlantillaAsync(flujo.Plantilla.Id, It.IsAny<CancellationToken>(), It.IsAny<bool>()))
            .ReturnsAsync(plantilla);
    }

    private void ConfigurarSolicitudYPlantilla(FlujoRestauracionApp flujo)
        => ConfigurarSolicitudYPlantilla(flujo, flujo.Plantilla);
}
