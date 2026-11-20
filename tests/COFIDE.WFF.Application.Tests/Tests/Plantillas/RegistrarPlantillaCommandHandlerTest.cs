using AutoMapper;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Commands.RegistrarPlantilla;
using COFIDE.WFF.Application.Features.Plantillas.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Services;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Inputs;
using FluentAssertions;
using Moq;

namespace COFIDE.WFF.Application.Tests.Tests.Plantillas;

public class RegistrarPlantillaCommandHandlerTest
{
    [Fact]
    public async Task Handle_PropagaAreaYAreaPadreDelUsuario_YConservaIdExitoso_RF01()
    {
        var request = new RegistrarPlantillaCommand(
            "Plantilla",
            "Descripción",
            false,
            false,
            null);
        var input = new CrearPlantillaInput
        {
            NombrePlantilla = "Plantilla",
            Descripcion = "Descripción",
            Visibilidad = false,
            EsReutilizable = false,
            Etapas = []
        };
        var plantilla = Plantilla.Crear(
            90,
            "Plantilla",
            "Descripción",
            false,
            15,
            "Gerencia",
            false);
        var mapper = new Mock<IMapper>();
        mapper.Setup(value => value.Map<CrearPlantillaInput>(request)).Returns(input);
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(value => value.UserIdLocal).Returns(15);
        currentUser.SetupGet(value => value.UserGerenciaNombre).Returns("Gerencia");
        currentUser.SetupGet(value => value.UserAreaId).Returns(31);
        currentUser.SetupGet(value => value.UserAreaPadreId).Returns(42);
        var repository = new Mock<IPlantillaRepository>();
        var creationService = new Mock<IPlantillaCreacionService>();
        creationService
            .Setup(service => service.CrearPlantilla(
                input,
                15,
                "Gerencia",
                31,
                42,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(plantilla);
        var handler = new RegistrarPlantillaCommandHandler(
            mapper.Object,
            currentUser.Object,
            repository.Object,
            creationService.Object);

        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().Be(90);
        creationService.Verify(service => service.CrearPlantilla(
            input,
            15,
            "Gerencia",
            31,
            42,
            It.IsAny<CancellationToken>()), Times.Once);
        repository.Verify(value => value.AddAsync(plantilla), Times.Once);
    }
}
