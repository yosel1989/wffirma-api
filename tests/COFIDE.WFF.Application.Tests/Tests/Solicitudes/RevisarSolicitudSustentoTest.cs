using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Maestros.DTOs;
using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.AprobarSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Services;
using COFIDE.WFF.Application.Features.Solicitudes.UseCases;
using COFIDE.WFF.Application.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Events;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Inputs;
using COFIDE.WFF.Domain.Common.Enums;
using COFIDE.WFF.Domain.DomainServices;
using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Domain.Exceptions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;

namespace COFIDE.WFF.Application.Tests.Features.Solicitudes;

public class RevisarSolicitudSustentoTest
{
    [Fact]
    public async Task Aprobar_Debe_Adjuntar_El_Sustento_Antes_De_Validar_Al_Firmante()
    {
        var solicitud = Borrador();
        var plantilla = Plantilla.Crear(5, "Flujo", "Descripcion", false, 10, "GTI", true);
        var enlace = SolicitudPlantilla.Crear(3, solicitud, plantilla);
        enlace.ActualizarPlantilla(5);
        solicitud.AsignarPlantilla(enlace);

        var solicitudes = new Mock<ISolicitudRepository>();
        var plantillas = new Mock<IPlantillaRepository>();
        var parametros = new Mock<IParametroRepository>();
        var sustentos = new Mock<ISolicitudSustentoService>();
        var correlativos = new Mock<IGeneradorCorrelativoIdRepository>();
        var eventos = new Mock<ISolicitudEventoFactory>();
        var flujo = new Mock<IWorkflowDomainService>();
        var archivo = new Mock<IFormFile>();
        flujo.Setup(f => f.ConstruirFirmanteFlujoEtapa(
                It.IsAny<Solicitud>(), It.IsAny<Plantilla>(), It.IsAny<Dictionary<long, ParametroDetalle>>()))
            .Returns(solicitud);

        solicitudes.Setup(s => s.ObtenerSolicitudParaAprobacion(8, It.IsAny<CancellationToken>(), true)).ReturnsAsync(solicitud);
        plantillas.Setup(p => p.ObtenerAgregadoPlantillaAsync(5, It.IsAny<CancellationToken>(), false)).ReturnsAsync(plantilla);
        parametros.Setup(p => p.ObtenerDetallePorCodigoParametro("TIPO_APROBADOR", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ParametroDetalleDto>());
        sustentos.Setup(s => s.CrearDocumentoSustentoInput(It.IsAny<List<IFormFile>?>(), solicitud.CodigoSolicitud, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SustentoProcesadoInput>
            {
                new() { NombreSustento = "anexo", ExtensionSustento = "pdf", RutaSustento = "sustentos", PesoSustento = 10 }
            });
        correlativos.Setup(c => c.ObtenerBloqueSiguienteIdSustentoAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<long> { 6 });
        eventos.Setup(e => e.CrearEventoSustento(8, 6))
            .Returns(new List<SolicitudEventoRegistradoDomainEvent>());

        var act = () => new RevisarSolicitudUseCase(
                Usuario(),
                solicitudes.Object,
                plantillas.Object,
                flujo.Object,
                parametros.Object,
                sustentos.Object,
                correlativos.Object,
                eventos.Object,
                new Mock<IFileStorageService>().Object)
            .Aprobar(new AprobarSolicitudCommand(8, null, [archivo.Object]), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
        solicitud.Sustentos.Should().ContainSingle(s => s.Id == 6 && s.NombreSustento == "anexo");
        solicitudes.Verify(s => s.GuardarFlujoSolicitudAsync(It.IsAny<Solicitud>()), Times.Never);
    }

    private static ICurrentUserService Usuario()
    {
        var usuario = new Mock<ICurrentUserService>();
        usuario.Setup(u => u.UserIdLocal).Returns(10L);
        usuario.Setup(u => u.UserGerenciaNombre).Returns("GTI");
        usuario.Setup(u => u.NombreUsuario).Returns("Ana");
        usuario.Setup(u => u.ApellidoPaterno).Returns("Perez");
        usuario.Setup(u => u.ApellidoMaterno).Returns("Lopez");
        usuario.Setup(u => u.RolUsuario).Returns(RolUsuario.SolicitanteAprobador);
        return usuario.Object;
    }

    private static Solicitud Borrador() =>
        Solicitud.CrearBorrador(8, "GTI-SOL-00000008-2026", 1, 2, 10, 3, 10, 5, 2,
            "Contrato", null, false, false, null, 0);
}
