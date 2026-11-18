using COFIDE.WFF.Api.Controllers.DTOs;
using COFIDE.WFF.Api.Controllers.v1;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerSolicitud;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace COFIDE.WFF.Api.Tests.Tests.Solicitudes;

public class ObtenerSolicitudOrigenTest
{
    [Theory]
    [InlineData(null)]
    [InlineData("mis_solicitudes")]
    [InlineData("seguimiento_solicitudes")]
    [InlineData("bandeja_entrada")]
    public async Task GetSolicitud_Debe_Enviar_Origen_Al_Query(string? origen)
    {
        using var cancelacion = new CancellationTokenSource();
        var dto = new SolicitudDto();
        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        mediator.Setup(m => m.Send(
                It.Is<ObtenerSolicitudQuery>(q => q.IdSolicitud == 140 && !q.EsReuso && q.Origen == origen),
                cancelacion.Token))
            .ReturnsAsync(dto);
        var controller = new SolicitudController(mediator.Object, new Mock<ICorrelationContext>().Object);

        var resultado = await controller.GetSolicitud(140, origen: origen, cancellationToken: cancelacion.Token);

        var ok = resultado.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<ResponseDto<SolicitudDto>>()
            .Which.Data.Should().BeSameAs(dto);
        mediator.Verify(m => m.Send(
            It.Is<ObtenerSolicitudQuery>(q => q.IdSolicitud == 140 && !q.EsReuso && q.Origen == origen),
            cancelacion.Token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }
}
