using AutoMapper;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Maestros.DTOs;
using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Services;
using COFIDE.WFF.Application.Features.Revisiones.EditarFlujo;
using COFIDE.WFF.Application.Features.Solicitudes.Models.Flujo;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Services;
using COFIDE.WFF.Application.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Inputs;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Events;
using COFIDE.WFF.Domain.DomainServices;
using COFIDE.WFF.Domain.Exceptions;
using FluentAssertions;
using Moq;
using Vo = COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Vo;

namespace COFIDE.WFF.Application.Tests.Features.Revisiones;

public class EditarFlujoRamaTest
{
    [Fact]
    public async Task Maestra_Debe_Crear_Una_Plantilla_Nueva_Y_Redirigir()
    {
        var solicitud = SolicitudConPlantilla(5, maestra: true);
        var nueva = Plantilla.Crear(9, "Copia", "Descripcion", false, 10, "GTI", false);
        var solicitudes = new Mock<ISolicitudRepository>();
        var plantillas = new Mock<IPlantillaRepository>();
        var creacion = new Mock<IPlantillaCreacionService>();
        var flujo = new Mock<IWorkflowDomainService>();
        var parametros = new Mock<IParametroRepository>();
        var enlace = new Mock<ISolicitudPlantillaRepository>();

        solicitudes.Setup(s => s.ObtenerAgregadoSolicitudAsync(8, It.IsAny<CancellationToken>(), true)).ReturnsAsync(solicitud);
        solicitudes.Setup(s => s.ObtenerSiguienteIdSolFirmanteAsync(It.IsAny<CancellationToken>())).ReturnsAsync(20);
        solicitudes.Setup(s => s.ObtenerBloqueSiguienteIdSolFirmanteAsync(0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<long>());
        plantillas.Setup(p => p.ObtenerAgregadoPlantillaAsync(5, It.IsAny<CancellationToken>(), true))
            .ReturnsAsync(solicitud.SolicitudPlantilla.Plantilla);
        plantillas.Setup(p => p.AddAsync(nueva)).Returns(Task.CompletedTask);
        creacion.Setup(c => c.CrearPlantilla(It.IsAny<CrearPlantillaInput>(), 10, "GTI", It.IsAny<CancellationToken>()))
            .ReturnsAsync(nueva);
        parametros.Setup(p => p.ObtenerDetallePorCodigoParametro("TIPO_APROBADOR", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ParametroDetalleDto>());
        enlace.Setup(e => e.UpdateAsync(solicitud.SolicitudPlantilla)).Returns(Task.CompletedTask);

        var respuesta = await Handler(solicitudes, plantillas, creacion, flujo, parametros, enlace)
            .Handle(new EditarFlujoCommand(8, 5, []), CancellationToken.None);

        respuesta.Redireccionar.Should().BeTrue();
        solicitud.SolicitudPlantilla.IdPlantilla.Should().Be(9);
        plantillas.Verify(p => p.AddAsync(nueva), Times.Once);
        flujo.Verify(f => f.ActualizarSolicitudFirmanteMaestroFlujo(solicitud.SolicitudPlantilla, nueva, 20), Times.Once);
        enlace.Verify(e => e.UpdateAsync(solicitud.SolicitudPlantilla), Times.Once);
    }

    [Fact]
    public async Task No_Maestra_Sin_Etapas_Debe_Lanzar_DOM_PLT_010()
    {
        var solicitud = SolicitudConPlantilla(5, maestra: false);
        var solicitudes = new Mock<ISolicitudRepository>();
        var plantillas = new Mock<IPlantillaRepository>();
        var parametros = new Mock<IParametroRepository>();
        var diffs = new Mock<ISolicitudFlujoDiffService>();
        var correlativos = new Mock<IGeneradorCorrelativoIdRepository>();

        solicitudes.Setup(s => s.ObtenerAgregadoSolicitudAsync(8, It.IsAny<CancellationToken>(), true)).ReturnsAsync(solicitud);
        solicitudes.Setup(s => s.ObtenerSiguienteIdSolFirmanteAsync(It.IsAny<CancellationToken>())).ReturnsAsync(20);
        solicitudes.Setup(s => s.ObtenerBloqueSiguienteIdSolFirmanteAsync(0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<long>());
        plantillas.Setup(p => p.ObtenerAgregadoPlantillaAsync(5, It.IsAny<CancellationToken>(), true))
            .ReturnsAsync(solicitud.SolicitudPlantilla.Plantilla);
        plantillas.Setup(p => p.ObtenerBloqueSiguienteIdEtapaAsync(0, It.IsAny<CancellationToken>())).ReturnsAsync(new List<long>());
        plantillas.Setup(p => p.ObtenerBloqueSiguienteIdFirmanteAsync(0, It.IsAny<CancellationToken>())).ReturnsAsync(new List<long>());
        parametros.Setup(p => p.ObtenerDetallePorCodigoParametro(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ParametroDetalleDto>());
        diffs.Setup(d => d.CalcularDiff(It.IsAny<Plantilla>(), It.IsAny<COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Models.PlantillaEditModel>()))
            .Returns(new SolicitudFlujoDiff());
        correlativos.Setup(c => c.ObtenerBloqueSiguienteIdDetalleFirmaAsync(0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<long>());

        var act = () => Handler(solicitudes, plantillas, parametros: parametros, diffs: diffs, correlativos: correlativos)
            .Handle(new EditarFlujoCommand(8, 5, []), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<DomainException>();
        ex.Which.Error.Code.Should().Be("DOM_PLT_010");
    }

    [Fact]
    public async Task No_Maestra_Debe_Agregar_La_Etapa_Y_Publicar_Su_Id()
    {
        var solicitud = SolicitudConPlantilla(5, maestra: false);
        var etapa = solicitud.SolicitudPlantilla.Plantilla.AgregarEtapas(
            3, 1, "Etapa 1", Vo.TipoEtapa.Secuencial, 1, Vo.TipoAprobador.Firmante, 2);
        etapa.AgregarFirmante(9, 100, null, null, null, 100);

        var solicitudes = new Mock<ISolicitudRepository>();
        var plantillas = new Mock<IPlantillaRepository>();
        var parametros = new Mock<IParametroRepository>();
        var eventos = new Mock<ISolicitudEventoFactory>();
        var enlace = new Mock<ISolicitudPlantillaRepository>();
        var correlativos = new Mock<IGeneradorCorrelativoIdRepository>();
        SolicitudFlujoDiff? publicado = null;

        solicitudes.Setup(s => s.ObtenerAgregadoSolicitudAsync(8, It.IsAny<CancellationToken>(), true)).ReturnsAsync(solicitud);
        solicitudes.Setup(s => s.ObtenerSiguienteIdSolFirmanteAsync(It.IsAny<CancellationToken>())).ReturnsAsync(20);
        solicitudes.Setup(s => s.ObtenerBloqueSiguienteIdSolFirmanteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<long> { 21 });
        plantillas.Setup(p => p.ObtenerAgregadoPlantillaAsync(5, It.IsAny<CancellationToken>(), true))
            .ReturnsAsync(solicitud.SolicitudPlantilla.Plantilla);
        plantillas.Setup(p => p.ObtenerBloqueSiguienteIdEtapaAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new List<long> { 15 });
        plantillas.Setup(p => p.ObtenerBloqueSiguienteIdFirmanteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new List<long> { 16 });
        parametros.Setup(p => p.ObtenerDetallePorCodigoParametro("TIPO_ETAPA", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ParametroDetalleDto> { new() { Id = 1, Codigo = "TI_ETAPA_SECUENCIAL", Valor = "SECUENCIAL" } });
        parametros.Setup(p => p.ObtenerDetallePorCodigoParametro("TIPO_APROBADOR", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ParametroDetalleDto> { new() { Id = 2, Codigo = "TI_APROBADOR_FIRMANTE", Valor = "FIRMANTE" } });
        correlativos.Setup(c => c.ObtenerBloqueSiguienteIdDetalleFirmaAsync(0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<long>());
        enlace.Setup(e => e.UpdateAsync(solicitud.SolicitudPlantilla)).Returns(Task.CompletedTask);
        eventos.Setup(e => e.CrearEventos(8, It.IsAny<SolicitudFlujoDiff>()))
            .Callback<long, SolicitudFlujoDiff>((_, diff) => publicado = diff)
            .Returns(new List<SolicitudEventoRegistradoDomainEvent>());

        var respuesta = await Handler(
                solicitudes, plantillas, parametros: parametros, enlace: enlace,
                diffs: null, correlativos: correlativos, eventos: eventos)
            .Handle(new EditarFlujoCommand(8, 5, new List<SolicitudEtapaRequest>
            {
                new(3, Guid.NewGuid(), 1, "Etapa 1", 1, 2, new List<SolicitudFirmanteFlujoRequest>
                {
                    new(9, 100, null, null, null, 100)
                }),
                new(0, Guid.Empty, 2, "Etapa 2", 1, 2, new List<SolicitudFirmanteFlujoRequest>
                {
                    new(0, 200, null, null, null, 200)
                })
            }), CancellationToken.None);

        respuesta.Redireccionar.Should().BeTrue();
        solicitud.SolicitudPlantilla.Plantilla.Etapas.Should().Contain(e => e.Id == 15 && e.OrdenEtapa == 2);
        publicado!.EtapasAgregadas.Should().ContainSingle(e => e.IdEtapa == 15 && e.Nombre == "Etapa 2");
        publicado.FirmantesAgregados.Should().Contain(f => f.IdUsuario == 200 && f.IdEtapa == 15);
        plantillas.Verify(p => p.GuardarFlujoPlantillaAsync(solicitud.SolicitudPlantilla.Plantilla), Times.Once);
    }

    private static EditarFlujoCommandHandler Handler(
        Mock<ISolicitudRepository> solicitudes,
        Mock<IPlantillaRepository> plantillas,
        Mock<IPlantillaCreacionService>? creacion = null,
        Mock<IWorkflowDomainService>? flujo = null,
        Mock<IParametroRepository>? parametros = null,
        Mock<ISolicitudPlantillaRepository>? enlace = null,
        Mock<ISolicitudFlujoDiffService>? diffs = null,
        Mock<IGeneradorCorrelativoIdRepository>? correlativos = null,
        Mock<ISolicitudEventoFactory>? eventos = null) =>
        new(
            new Mock<IMapper>().Object,
            Usuario(),
            solicitudes.Object,
            (enlace ?? new Mock<ISolicitudPlantillaRepository>()).Object,
            plantillas.Object,
            (creacion ?? new Mock<IPlantillaCreacionService>()).Object,
            (flujo ?? new Mock<IWorkflowDomainService>()).Object,
            (parametros ?? new Mock<IParametroRepository>()).Object,
            diffs?.Object ?? new SolicitudFlujoDiffService(),
            (eventos ?? new Mock<ISolicitudEventoFactory>()).Object,
            (correlativos ?? new Mock<IGeneradorCorrelativoIdRepository>()).Object);

    private static Solicitud SolicitudConPlantilla(long idPlantilla, bool maestra)
    {
        var solicitud = Solicitud.CrearBorrador(8, "GTI-SOL-00000008-2026", 1, 2, 10, 3, 10, 5, 2,
            "Contrato", null, false, false, null, 0);
        var plantilla = Plantilla.Crear(idPlantilla, "Flujo", "Descripcion", maestra, 10, "GTI", maestra);
        var enlace = SolicitudPlantilla.Crear(3, solicitud, plantilla);
        enlace.ActualizarPlantilla(idPlantilla);
        solicitud.AsignarPlantilla(enlace);
        return solicitud;
    }

    private static ICurrentUserService Usuario()
    {
        var usuario = new Mock<ICurrentUserService>();
        usuario.Setup(u => u.UserIdLocal).Returns(10L);
        usuario.Setup(u => u.UserGerenciaNombre).Returns("GTI");
        return usuario.Object;
    }
}
