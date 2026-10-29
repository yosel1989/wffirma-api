using COFIDE.WFF.Api.Constants;
using COFIDE.WFF.Api.Controllers.DTOs;
using COFIDE.WFF.Api.Controllers.v1;
using COFIDE.WFF.Api.Request.Solicitudes;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.RestaurarSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerPuntosRestauracion;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace COFIDE.WFF.Api.Tests.Tests.Solicitudes;

public class RestauracionFlujoTest
{
    [Fact]
    public async Task ObtenerPuntosRestauracion_Debe_Enviar_La_Query_Y_Responder_Ok()
    {
        var puntos = new List<PuntoRestauracionDto>
        {
            new()
            {
                IdPuntoRestauracion = 1718,
                Nombre = "Ana Perez",
                Rol = "Firmante",
                Fecha = "2026-10-29T12:33:20",
                Aprobado = true
            }
        };
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.Is<ObtenerPuntosRestauracionQuery>(q => q.IdSolicitud == 8), It.IsAny<CancellationToken>()))
            .ReturnsAsync(puntos);

        var actionResult = await Controller(mediator).ObtenerPuntosRestauracion(8, CancellationToken.None);

        var ok = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var body = ok.Value.Should().BeOfType<ResponseDto<IReadOnlyList<PuntoRestauracionDto>>>().Subject;
        body.Success.Should().BeTrue();
        body.Message.Should().Be(ResponseCodes.OperacionExitosa);
        body.Data.Should().BeEquivalentTo(puntos);

        mediator.Verify(m => m.Send(
            It.Is<ObtenerPuntosRestauracionQuery>(q => q.IdSolicitud == 8),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RestaurarSolicitud_Debe_Enviar_El_Command_Y_Responder_Ok()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(
                It.Is<RestaurarSolicitudCommand>(c => c.Id == 8 && c.IdPuntoRestauracion == 1718),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("GTI-SOL-00000008-2026");

        var actionResult = await Controller(mediator).RestaurarSolicitud(
            8,
            new RestaurarSolicitudRequest { IdPuntoRestauracion = 1718 },
            CancellationToken.None);

        var ok = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var body = ok.Value.Should().BeOfType<ResponseDto<string?>>().Subject;
        body.Success.Should().BeTrue();
        body.Data.Should().Be("GTI-SOL-00000008-2026");
        body.Message.Should().Be(ResponseCodes.OperacionExitosa);

        mediator.Verify(m => m.Send(
            It.Is<RestaurarSolicitudCommand>(c => c.Id == 8 && c.IdPuntoRestauracion == 1718),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private static SolicitudController Controller(Mock<IMediator> mediator) =>
        new(mediator.Object, new Mock<ICorrelationContext>().Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
}
