using COFIDE.WFF.Api.Constants;
using COFIDE.WFF.Api.Controllers.DTOs;
using COFIDE.WFF.Api.Controllers.v1;
using COFIDE.WFF.Application.Common.Interfaces;
using COFIDE.WFF.Application.Features.Firmas.Commands.RestablecerDocumentoFirmado;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
namespace COFIDE.WFF.Api.Tests.Firmas.RestablecerFirma;
public class RestablecerFirmaTest
{

    [Fact]
    public async Task ReestablecerDocumentosFirmados_Debe_Enviar_Command_Y_Retornar_ResponseDto_Ok()
    {
        var ids = new List<long> { 10, 20 };
        var request = new RestablecerDocumentoFirmadoRequest(ids);
        var mediatorMock = new Mock<IMediator>();
        mediatorMock
        .Setup(m => m.Send(
            It.Is<RestablecerDocumentoFirmadoCommand>(c =>
                c.IdDetalleFirma.SequenceEqual(ids)),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync("");

        var correlationMock = new Mock<ICorrelationContext>();
        var controller = new FirmaController(mediatorMock.Object, correlationMock.Object)
        {
            ControllerContext = new ControllerContext

            {
                HttpContext = new DefaultHttpContext()
            }

        };

        var actionResult = await controller.ReestablecerDocumentosFirmados(request);

        var ok = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var body = ok.Value.Should().BeOfType<ResponseDto<string?>>().Subject;

        body.Success.Should().BeTrue();
        body.Data.Should().Be("");
        body.Message.Should().Be(ResponseCodes.OperacionExitosa);
        body.CorrelationId.Should().Be(controller.HttpContext.TraceIdentifier);

        mediatorMock.Verify(m => m.Send(
            It.IsAny<RestablecerDocumentoFirmadoCommand>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

}
