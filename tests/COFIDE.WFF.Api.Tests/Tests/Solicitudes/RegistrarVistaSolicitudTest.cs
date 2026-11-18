using COFIDE.WFF.Api.Controllers.DTOs;
using COFIDE.WFF.Api.Controllers.v1;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.RegistrarVistaSolicitud;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace COFIDE.WFF.Api.Tests.Tests.Solicitudes;

public class RegistrarVistaSolicitudTest
{
    [Fact]
    public async Task Endpoint_EnviaIdYCancelacionYDevuelveLaFechaRegistrada()
    {
        var fecha = new DateTime(2026, 11, 17, 12, 0, 0);
        using var cancelacion = new CancellationTokenSource();
        var mediator = new Mock<IMediator>();
        mediator.Setup(x => x.Send(new RegistrarVistaSolicitudCommand(8), cancelacion.Token)).ReturnsAsync(fecha);
        var controller = new SolicitudController(mediator.Object, new Mock<ICorrelationContext>().Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        var resultado = await controller.RegistrarVistaSolicitud(8, cancelacion.Token);

        var ok = Assert.IsType<OkObjectResult>(resultado.Result);
        var respuesta = Assert.IsType<ResponseDto<DateTime>>(ok.Value);
        Assert.True(respuesta.Success);
        Assert.Equal(fecha, respuesta.Data);
        Assert.Equal(controller.HttpContext.TraceIdentifier, respuesta.CorrelationId);
        mediator.Verify(x => x.Send(new RegistrarVistaSolicitudCommand(8), cancelacion.Token), Times.Once);
        var atributo = Assert.Single(typeof(SolicitudController).GetMethod(nameof(SolicitudController.RegistrarVistaSolicitud))!
            .GetCustomAttributes(typeof(HttpPutAttribute), false).Cast<HttpPutAttribute>());
        Assert.Equal("{id}/visualizacion", atributo.Template);
    }
}
