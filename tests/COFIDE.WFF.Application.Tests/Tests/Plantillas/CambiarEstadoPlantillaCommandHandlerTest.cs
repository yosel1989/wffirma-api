using AutoMapper;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Commands.CambiarEstadoPlantilla;
using COFIDE.WFF.Application.Features.Plantillas.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Services;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Common.Enums;
using FluentAssertions;
using Moq;

namespace COFIDE.WFF.Application.Tests.Plantillas;

public class CambiarEstadoPlantillaCommandHandlerTest
{
    [Fact]
    public async Task CambiarEstado_DebePermitirDesactivarYReactivarLaPlantilla()
    {
        var plantilla = Plantilla.Crear(1, "Plantilla", "Descripción", false, 10, "GTI", true);
        var repository = new Mock<IPlantillaRepository>();
        repository
            .Setup(x => x.ObtenerParaActivacionAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plantilla);

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.UserIdLocal).Returns(10);
        currentUser.Setup(x => x.RolUsuario).Returns(RolUsuario.SolicitanteAprobador);
        currentUser.Setup(x => x.UserGerenciaNombre).Returns("GTI");

        var handler = new CambiarEstadoPlantillaCommandHandler(
            new Mock<IMapper>().Object,
            repository.Object,
            new PlantillaCambioEstadoAuthorizationService(
                currentUser.Object,
                new Mock<IAccesoEspecialRepository>().Object));

        var estadoDesactivado = await handler.Handle(
            new CambiarEstadoPlantillaCommand(1, false),
            CancellationToken.None);
        var estadoActivado = await handler.Handle(
            new CambiarEstadoPlantillaCommand(1, true),
            CancellationToken.None);

        estadoDesactivado.Should().BeFalse();
        estadoActivado.Should().BeTrue();
        repository.Verify(x => x.UpdateAsync(plantilla), Times.Exactly(2));
    }
}
