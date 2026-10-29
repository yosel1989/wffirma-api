using AutoMapper;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.RestaurarSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Services;
using FluentAssertions;
using Moq;

namespace COFIDE.WFF.Application.Tests.Solicitudes;

public class RestaurarSolicitudCommandHandlerTest
{
    [Fact]
    public async Task Handle_Debe_Delegar_Al_Servicio_Y_Devolver_El_Codigo()
    {
        var usecase = new Mock<ISolicitudRestauracionFlujoService>();
        var comando = new RestaurarSolicitudCommand(8, 501);
        usecase
            .Setup(s => s.Restaurar(comando, It.IsAny<CancellationToken>()))
            .ReturnsAsync("GTI-SOL-00000008-2026");

        var resultado = await new RestaurarSolicitudCommandHandler(new Mock<IMapper>().Object, usecase.Object)
            .Handle(comando, CancellationToken.None);

        resultado.Should().Be("GTI-SOL-00000008-2026");
        usecase.Verify(s => s.Restaurar(comando, It.IsAny<CancellationToken>()), Times.Once);
    }
}
