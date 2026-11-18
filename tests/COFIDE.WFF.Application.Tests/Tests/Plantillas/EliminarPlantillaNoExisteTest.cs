using AutoMapper;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Features.Plantillas.Commands.EliminarPlantilla;
using COFIDE.WFF.Application.Features.Plantillas.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Services;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using Moq;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;

namespace COFIDE.WFF.Application.Tests.Tests.Plantillas;

public class EliminarPlantillaNoExisteTest
{
    [Fact]
    public async Task Handle_ConPlantillaInexistente_IndicaQueYaNoExiste_RF01()
    {
        var repository = new Mock<IPlantillaRepository>();
        repository.Setup(repo => repo.ObtenerParaCambioEstadoAsync(
                99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Plantilla?)null);
        var handler = CrearHandler(repository);

        var exception = await Assert.ThrowsAsync<ValidacionAplicacion.ApplicationException>(
            () => handler.Handle(
                new EliminarPlantillaCommand(99),
                CancellationToken.None));

        Assert.Equal(ApplicationErrors.PlantillaYaNoExiste, exception.Error);
        Assert.Equal("La plantilla ya no existe", exception.Error.Description);
        repository.Verify(repo => repo.UpdateAsync(It.IsAny<Plantilla>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ConPlantillaYaEliminada_IndicaQueYaNoExiste_RF01()
    {
        var plantilla = Plantilla.Crear(
            1, "Plantilla", "Descripción", false, 10, "GTI", true);
        plantilla.EliminarLogicamente();
        var repository = new Mock<IPlantillaRepository>();
        var authorization = new Mock<IPlantillaCambioEstadoAuthorizationService>();
        repository.Setup(repo => repo.ObtenerParaCambioEstadoAsync(
                1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plantilla);
        var handler = new EliminarPlantillaCommandHandler(
            Mock.Of<IMapper>(),
            repository.Object,
            authorization.Object);

        var exception = await Assert.ThrowsAsync<ValidacionAplicacion.ApplicationException>(
            () => handler.Handle(
                new EliminarPlantillaCommand(1),
                CancellationToken.None));

        Assert.Equal(ApplicationErrors.PlantillaYaNoExiste, exception.Error);
        Assert.Equal("La plantilla ya no existe", exception.Error.Description);
        repository.Verify(repo => repo.UpdateAsync(It.IsAny<Plantilla>()), Times.Never);
        authorization.Verify(
            service => service.AutorizarAsync(plantilla, It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static EliminarPlantillaCommandHandler CrearHandler(
        Mock<IPlantillaRepository> repository) =>
        new(
            Mock.Of<IMapper>(),
            repository.Object,
            Mock.Of<IPlantillaCambioEstadoAuthorizationService>());
}
