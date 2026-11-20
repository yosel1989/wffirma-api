using AutoMapper;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Commands.RegistrarPlantilla;
using COFIDE.WFF.Application.Features.Plantillas.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Services;
using COFIDE.WFF.Application.Exceptions;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Inputs;
using Moq;

namespace COFIDE.WFF.Application.Tests.Tests.Plantillas;

public class RegistrarPlantillaAreaIncompletaHandlerTest
{
    [Fact]
    public Task Handle_SinArea_RechazaSinCrearNiPersistir_RF02() =>
        RechazarSinTerritorioCompleto(idArea: null, idAreaPadre: 42);

    [Fact]
    public Task Handle_SinAreaPadre_RechazaSinCrearNiPersistir_RF02() =>
        RechazarSinTerritorioCompleto(idArea: 31, idAreaPadre: null);

    private static async Task RechazarSinTerritorioCompleto(long? idArea, long? idAreaPadre)
    {
        var request = new RegistrarPlantillaCommand(
            "Plantilla",
            "Descripción",
            false,
            false,
            null);
        var mapper = new Mock<IMapper>();
        mapper.Setup(value => value.Map<CrearPlantillaInput>(request))
            .Returns(new CrearPlantillaInput { Etapas = [] });
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(value => value.UserIdLocal).Returns(15);
        currentUser.SetupGet(value => value.UserGerenciaNombre).Returns("Gerencia");
        currentUser.SetupGet(value => value.UserAreaId).Returns(idArea);
        currentUser.SetupGet(value => value.UserAreaPadreId).Returns(idAreaPadre);
        var repository = new Mock<IPlantillaRepository>();
        var creationService = new Mock<IPlantillaCreacionService>();
        var handler = new RegistrarPlantillaCommandHandler(
            mapper.Object,
            currentUser.Object,
            repository.Object,
            creationService.Object);

        var exception = await Assert.ThrowsAsync<COFIDE.WFF.Application.Exceptions.ApplicationException>(
            () => handler.Handle(request, CancellationToken.None));

        Assert.Equal("APP_PLT_002", exception.Error.Code);
        Assert.Equal(400, exception.HttpStatusCode);
        creationService.VerifyNoOtherCalls();
        repository.Verify(
            value => value.AddAsync(It.IsAny<Plantilla>()),
            Times.Never);
    }
}
