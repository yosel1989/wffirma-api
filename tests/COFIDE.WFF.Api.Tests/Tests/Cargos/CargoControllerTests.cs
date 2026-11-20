using COFIDE.WFF.Api.Constants;
using COFIDE.WFF.Api.Controllers.DTOs;
using COFIDE.WFF.Api.Controllers.v1;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Cargos.Queries.ObtenerCargosCombo;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace COFIDE.WFF.Api.Tests.Tests.Cargos;

/// <summary>Pruebas de las respuestas del combo de cargos.</summary>
public class CargoControllerTests
{
    /// <summary>Comprueba la respuesta exitosa y el envío de la consulta.</summary>
    [Fact]
    public async Task ObtenerCombo_ConCargos_DevuelveOk()
    {
        var mediator = new Mock<IMediator>();
        var cargos = new List<ObtenerCargosDto> { new() { IdCargo = 7, NombreCargo = "Analista" } };
        using var cts = new CancellationTokenSource();
        mediator.Setup(m => m.Send(It.IsAny<ObtenerCargosQuery>(), cts.Token)).ReturnsAsync(cargos);
        var controller = new CargoController(mediator.Object, Mock.Of<ICorrelationContext>());

        var result = await controller.ObtenerCombo(cts.Token);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ResponseDto<List<ObtenerCargosDto>>>(ok.Value);
        Assert.True(response.Success);
        var cargo = Assert.Single(Assert.IsType<List<ObtenerCargosDto>>(response.Data));
        Assert.Equal(7, cargo.IdCargo);
        Assert.Equal("Analista", cargo.NombreCargo);
        mediator.Verify(m => m.Send(It.IsAny<ObtenerCargosQuery>(), cts.Token), Times.Once);
    }

    /// <summary>Comprueba la respuesta de registros no encontrados.</summary>
    [Fact]
    public async Task ObtenerCombo_SinCargos_DevuelveNotFound()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<ObtenerCargosQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ObtenerCargosDto>());
        var controller = new CargoController(mediator.Object, Mock.Of<ICorrelationContext>());

        var result = await controller.ObtenerCombo(CancellationToken.None);

        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        var response = Assert.IsType<ResponseDto<List<ObtenerCargosDto>>>(notFound.Value);
        Assert.False(response.Success);
        Assert.Equal(ResponseCodes.NotFound, response.Code);
        Assert.Equal(ResponseCodes.RegistrosNoEncontrados, response.Message);
        Assert.Null(response.Data);
    }
}
