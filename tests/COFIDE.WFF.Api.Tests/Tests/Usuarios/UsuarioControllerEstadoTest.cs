using COFIDE.WFF.Api.Constants;
using COFIDE.WFF.Api.Controllers.DTOs;
using COFIDE.WFF.Api.Controllers.v1;
using COFIDE.WFF.Api.Request.Usuarios;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Usuarios.Commands.CambiarEstadoUsuario;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Text.Json;

namespace COFIDE.WFF.Api.Tests.Tests.Usuarios;

public sealed class UsuarioControllerEstadoTest
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CambiarEstadoUsuario_MapeaSolicitudYDevuelveEstadoConExito(bool estado)
    {
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        mediator.Setup(x => x.Send(It.Is<CambiarEstadoUsuarioCommand>(c => c.Id == 7 && c.Estado == estado), token))
            .ReturnsAsync(estado);
        var controller = new UsuarioController(mediator.Object, Mock.Of<ICorrelationContext>())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { TraceIdentifier = "usuario-estado-7" }
            }
        };

        var result = await controller.CambiarEstadoUsuario(7, new() { Estado = estado }, token);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);
        var response = Assert.IsType<ResponseDto<bool>>(ok.Value);
        Assert.True(response.Success);
        Assert.Equal(estado, response.Data);
        Assert.Equal(ResponseCodes.Success, response.Code);
        Assert.Equal(ResponseCodes.OperacionExitosa, response.Message);
        Assert.Equal("usuario-estado-7", response.CorrelationId);
        mediator.VerifyAll();
        mediator.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"estado\":null}")]
    [InlineData("{\"estado\":\"false\"}")]
    public void Request_EstadoAusenteOInvalido_RechazaJson(string json)
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<CambiarEstadoUsuarioRequest>(
            json, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
    }

    [Theory]
    [InlineData("{\"estado\":true}", true)]
    [InlineData("{\"estado\":false}", false)]
    public void Request_EstadoExplicito_DeserializaAmbosValores(string json, bool estado)
    {
        var request = JsonSerializer.Deserialize<CambiarEstadoUsuarioRequest>(
            json, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.NotNull(request);
        Assert.Equal(estado, request.Estado);
    }

    [Fact]
    public void CambiarEstadoUsuario_ExponeRutaPutEsperada()
    {
        var method = typeof(UsuarioController).GetMethod(nameof(UsuarioController.CambiarEstadoUsuario))!;
        var route = Assert.Single(method.GetCustomAttributes(typeof(HttpPutAttribute), false));

        Assert.Equal("{id:long}/estado", Assert.IsType<HttpPutAttribute>(route).Template);
    }
}
