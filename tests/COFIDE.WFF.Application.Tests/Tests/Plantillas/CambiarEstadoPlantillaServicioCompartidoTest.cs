using AutoMapper;
using COFIDE.WFF.Application.Features.Plantillas.Commands.CambiarEstadoPlantilla;
using COFIDE.WFF.Application.Features.Plantillas.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Services;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using Moq;

namespace COFIDE.WFF.Application.Tests.Tests.Plantillas;

public class CambiarEstadoPlantillaServicioCompartidoTest
{
    [Fact]
    public async Task Handle_DelegaAutorizacionYConservaEstadosIdempotentes_RF02_RF06()
    {
        var plantilla = Plantilla.Crear(
            1, "Plantilla", "Descripción", false, 10, "GTI", true);
        var repository = new Mock<IPlantillaRepository>();
        repository.Setup(repo => repo.ObtenerParaActivacionAsync(
                1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plantilla);
        var authorization = new Mock<IPlantillaCambioEstadoAuthorizationService>();
        var handler = new CambiarEstadoPlantillaCommandHandler(
            Mock.Of<IMapper>(),
            repository.Object,
            authorization.Object);

        var primeraDesactivacion = await handler.Handle(
            new CambiarEstadoPlantillaCommand(1, false),
            CancellationToken.None);
        var segundaDesactivacion = await handler.Handle(
            new CambiarEstadoPlantillaCommand(1, false),
            CancellationToken.None);
        var activacion = await handler.Handle(
            new CambiarEstadoPlantillaCommand(1, true),
            CancellationToken.None);

        Assert.False(primeraDesactivacion);
        Assert.False(segundaDesactivacion);
        Assert.True(activacion);
        authorization.Verify(service => service.AutorizarAsync(
            plantilla, CancellationToken.None), Times.Exactly(3));
        repository.Verify(repo => repo.UpdateAsync(plantilla),
            Times.Exactly(3));
    }
}
