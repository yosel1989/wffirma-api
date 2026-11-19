using COFIDE.WFF.Api.Controllers.DTOs;
using COFIDE.WFF.Api.Controllers.v1;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Commands.EliminarPlantilla;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace COFIDE.WFF.Api.Tests.Tests.Plantillas;

public class PlantillaControllerEliminarTest
{
    [Fact]
    public async Task EliminarPlantilla_DevuelveRespuestaExitosa_RF01()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(service => service.Send(
                It.IsAny<EliminarPlantillaCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var controller = new PlantillaController(
            mediator.Object,
            Mock.Of<ICorrelationContext>())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        var resultado = await controller.EliminarPlantilla(
            25,
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(resultado.Result);
        var respuesta = Assert.IsType<ResponseDto<bool>>(ok.Value);
        Assert.True(respuesta.Success);
        Assert.True(respuesta.Data);
        mediator.Verify(service => service.Send(
            It.Is<EliminarPlantillaCommand>(command => command.Id == 25),
            CancellationToken.None), Times.Once);
    }

    [Fact]
    public void EliminarPlantilla_DeclaraRutaYErroresHttp_RF01()
    {
        var metodo = typeof(PlantillaController)
            .GetMethod(nameof(PlantillaController.EliminarPlantilla));
        Assert.NotNull(metodo);

        var delete = Assert.Single(
            metodo!.GetCustomAttributes(typeof(HttpDeleteAttribute), false)
                .Cast<HttpDeleteAttribute>());
        Assert.Equal("{idPlantilla:long}", delete.Template);

        var estados = metodo
            .GetCustomAttributes(typeof(ProducesResponseTypeAttribute), false)
            .Cast<ProducesResponseTypeAttribute>()
            .Select(attribute => attribute.StatusCode);
        Assert.Contains(StatusCodes.Status200OK, estados);
        Assert.Contains(StatusCodes.Status400BadRequest, estados);
        Assert.Contains(StatusCodes.Status404NotFound, estados);
        Assert.Contains(StatusCodes.Status409Conflict, estados);
    }
}
