using AutoMapper;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Maestros.DTOs;
using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Commands.EditarPlantilla;
using COFIDE.WFF.Application.Features.Plantillas.Models;
using COFIDE.WFF.Application.Features.Plantillas.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Common.Enums;
using FluentAssertions;
using Moq;
using Vo = COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Vo;

namespace COFIDE.WFF.Application.Tests.Plantillas;

public class EditarPlantillaExitoTest
{
    [Fact]
    public async Task Editar_Conservando_La_Etapa_Debe_Guardar_El_Flujo()
    {
        var plantilla = Plantilla.Crear(6, "Flujo", "Descripcion", false, 10, "GTI", true);
        var etapa = plantilla.AgregarEtapas(3, 1, "Etapa 1", Vo.TipoEtapa.Secuencial, 1, Vo.TipoAprobador.Firmante, 2);
        etapa.AgregarFirmante(9, 100, "Analista", "GTI", "GERENCIA", 100);

        var plantillas = new Mock<IPlantillaRepository>();
        var parametros = new Mock<IParametroRepository>();
        var usuario = new Mock<ICurrentUserService>();
        plantillas.Setup(p => p.ObtenerAgregadoPlantillaAsync(6, It.IsAny<CancellationToken>(), true)).ReturnsAsync(plantilla);
        plantillas.Setup(p => p.ObtenerBloqueSiguienteIdEtapaAsync(0, It.IsAny<CancellationToken>())).ReturnsAsync(new List<long>());
        plantillas.Setup(p => p.ObtenerBloqueSiguienteIdFirmanteAsync(0, It.IsAny<CancellationToken>())).ReturnsAsync(new List<long>());
        parametros.Setup(p => p.ObtenerDetallePorCodigoParametro("TIPO_ETAPA", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ParametroDetalleDto> { new() { Id = 1, Codigo = "TI_ETAPA_SECUENCIAL", Valor = "SECUENCIAL" } });
        parametros.Setup(p => p.ObtenerDetallePorCodigoParametro("TIPO_APROBADOR", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ParametroDetalleDto> { new() { Id = 2, Codigo = "TI_APROBADOR_FIRMANTE", Valor = "FIRMANTE" } });
        usuario.Setup(u => u.UserIdLocal).Returns(10L);
        usuario.Setup(u => u.UserGerenciaNombre).Returns("GTI");
        usuario.Setup(u => u.RolUsuario).Returns(RolUsuario.SolicitanteAprobador);

        var id = await new EditarPlantillaCommandHandler(
                new Mock<IMapper>().Object, usuario.Object, plantillas.Object, parametros.Object)
            .Handle(new EditarPlantillaCommand(6, "Flujo editado", "Descripcion", new List<PlantillaEtapaRequest>
            {
                new()
                {
                    Id = 3,
                    OrdenEtapa = 1,
                    NombreEtapa = "Etapa 1",
                    IdTipoEtapa = 1,
                    IdTipoAprobador = 2,
                    Firmantes = new List<FirmanteFlujoRequest>
                    {
                        new() { Id = 9, IdUsuarioAsignado = 100, IdUsuarioOrigen = 100, CargoUsuario = "Analista" }
                    }
                }
            }, true), CancellationToken.None);

        id.Should().Be(6);
        plantilla.NombrePlantilla!.Valor.Should().Be("Flujo editado");
        plantilla.Descripcion!.Valor.Should().Be("Descripcion");
        plantilla.Visibilidad.Should().BeTrue();
        plantilla.Etapas.Should().ContainSingle(e => e.Id == 3 && e.Estado);
        plantillas.Verify(p => p.GuardarFlujoPlantillaAsync(plantilla), Times.Once);
    }
}
