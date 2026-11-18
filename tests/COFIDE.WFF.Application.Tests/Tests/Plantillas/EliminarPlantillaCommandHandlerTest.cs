using AutoMapper;
using COFIDE.WFF.Application.Features.Plantillas.Commands.EliminarPlantilla;
using COFIDE.WFF.Application.Features.Plantillas.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Services;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Vo;
using Moq;

namespace COFIDE.WFF.Application.Tests.Tests.Plantillas;

public class EliminarPlantillaCommandHandlerTest
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Handle_ConPlantillaActivaOInactiva_ConservaDatosYElimina_RF01(
        bool activa)
    {
        var plantilla = Plantilla.Crear(
            1, "Plantilla", "Descripción", false, 10, "GTI", true);
        var etapa = plantilla.AgregarEtapas(
            2, 1, "Etapa", TipoEtapa.Secuencial, 1, TipoAprobador.Firmante, 10);
        etapa.AgregarFirmante(
            3, 20, "Cargo", "Área", "GTI", 10);
        var firmante = Assert.Single(etapa.Firmantes);
        if (!activa)
            plantilla.Desactivar();

        var nombre = plantilla.NombrePlantilla;
        var descripcion = plantilla.Descripcion;
        var repository = new Mock<IPlantillaRepository>();
        repository.Setup(repo => repo.ObtenerParaCambioEstadoAsync(
                1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plantilla);
        var authorization = new Mock<IPlantillaCambioEstadoAuthorizationService>();
        var handler = new EliminarPlantillaCommandHandler(
            Mock.Of<IMapper>(),
            repository.Object,
            authorization.Object);

        var resultado = await handler.Handle(
            new EliminarPlantillaCommand(1),
            CancellationToken.None);

        Assert.True(resultado);
        Assert.Equal(EstadoPlantilla.Eliminada, plantilla.EstadoPlantilla);
        Assert.Same(nombre, plantilla.NombrePlantilla);
        Assert.Same(descripcion, plantilla.Descripcion);
        Assert.Same(etapa, Assert.Single(plantilla.Etapas));
        Assert.Same(firmante, Assert.Single(etapa.Firmantes));
        repository.Verify(repo => repo.UpdateAsync(plantilla), Times.Once);
    }
}
