using AutoMapper;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Firmas.DTOs;
using COFIDE.WFF.Application.Features.Firmas.Services;
using COFIDE.WFF.Application.Features.Jobs.ActualizarEstadoProgramado;
using COFIDE.WFF.Application.Features.Maestros.DTOs;
using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Commands.EditarPlantilla;
using COFIDE.WFF.Application.Features.Plantillas.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Common.Enums;
using COFIDE.WFF.Domain.Exceptions;
using FluentAssertions;
using Moq;
using AppEx = COFIDE.WFF.Application.Exceptions;

using COFIDE.WFF.Application.Tests.Support;
namespace COFIDE.WFF.Application.Tests.Plantillas;

public class PlantillaYJobHandlerTest
{
    [Fact]
    public async Task EditarPlantilla_De_Otro_Solicitante_Debe_Lanzar_DOM_PLT_027()
    {
        var usuario = new Mock<ICurrentUserService>();
        var plantillas = new Mock<IPlantillaRepository>();
        var parametros = new Mock<IParametroRepository>();
        var plantilla = Plantilla.Crear(6, "Flujo", "Descripcion", false, 10, "GTI", true);
        usuario.Setup(u => u.UserIdLocal).Returns(99L);
        usuario.Setup(u => u.UserGerenciaNombre).Returns("GTI");
        usuario.Setup(u => u.RolUsuario).Returns(RolUsuario.SolicitanteAprobador);
        plantillas.Setup(p => p.ObtenerAgregadoPlantillaAsync(6, It.IsAny<CancellationToken>(), true))
            .ReturnsAsync(plantilla);

        var act = () => new EditarPlantillaCommandHandler(
                new Mock<IMapper>().Object, usuario.Object, plantillas.Object, parametros.Object)
            .Handle(new EditarPlantillaCommand(6, "Flujo", "Descripcion", []), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<DomainException>();
        ex.Which.Error.Code.Should().Be("DOM_PLT_027");
        parametros.Verify(p => p.ObtenerDetallePorCodigoParametro(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Job_Debe_Consultar_El_Parametro_Y_Responder_Uno()
    {
        var parametros = new Mock<IParametroRepository>();
        parametros.Setup(p => p.ObtenerParametroDetallePorIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ParametroDetalle?)null);

        var cantidad = await new ActualizarEstadosProgramadoCommandHandler(parametros.Object)
            .Handle(new ActualizarEstadosProgramadoCommand(7), CancellationToken.None);

        cantidad.Should().Be(1);
    }

    [Fact]
    public async Task Preparar_Firma_Sin_Firmante_Asignado_Debe_Lanzar_APP_SOL_006()
    {
        var solicitud = Borrador();
        var plantilla = Plantilla.Crear(6, "Flujo", "Descripcion", false, 10, "GTI", true);
        solicitud.AsignarPlantilla(SolicitudPlantilla.Crear(3, solicitud, plantilla));
        solicitud.AgregarDocumento(4, "contrato", "pdf", "originales", "hash",
            null, null, null, null, null, null, null, null, 10);
        var documento = solicitud.Documentos.Single();
        NavegacionEf.Asignar(documento, x => x.Solicitud, solicitud);
        var plantillas = new Mock<IPlantillaRepository>();
        var parametros = new Mock<IParametroRepository>();
        plantillas.Setup(p => p.ObtenerAgregadoPlantillaAsync(It.IsAny<long>(), It.IsAny<CancellationToken>(), true))
            .ReturnsAsync(plantilla);
        parametros.Setup(p => p.ObtenerDetallePorCodigoParametro("TIPO_APROBADOR", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ParametroDetalleDto>());
        parametros.Setup(p => p.ObtenerTiempoProcesoFirmadoArchivoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TiempoProcesoFirmaDto());
        parametros.Setup(p => p.ObtenerTamanioFirmaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TamanioFirmaDto());
        parametros.Setup(p => p.ObtenerPosicionFirmaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PosicionFirmaDto());
        parametros.Setup(p => p.ConfiguracionFirmaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConfiguracionFirmaDto());

        var act = () => new FirmaPreparationService(
                new Mock<COFIDE.WFF.Domain.DomainServices.IWorkflowDomainService>().Object,
                plantillas.Object,
                parametros.Object)
            .PrepararDocumentoFirmaAsync(documento, "REF", 10, 1, null, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be("APP_SOL_006");
    }

    private static Solicitud Borrador() =>
        Solicitud.CrearBorrador(8, "GTI-SOL-00000008-2026", 1, 2, 10, 3, 10, 5, 2,
            "Contrato", null, false, false, null, 0);
}
