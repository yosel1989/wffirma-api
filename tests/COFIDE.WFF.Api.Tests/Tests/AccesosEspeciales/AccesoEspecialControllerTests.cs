using Moq;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using COFIDE.WFF.Api.Controllers.v1;
using COFIDE.WFF.Api.Request.AccesosEspeciales;
using COFIDE.WFF.Api.Controllers.DTOs;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Queries.ObtenerAccesoEspecialPaginado;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Commands.CambiarEstadoAccesoEspecial;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Commands.EliminarAccesoEspecial;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Queries.ValidarExisteAccesoEspecial;

public class AccesoEspecialControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly AccesoEspecialController _controller;

    public AccesoEspecialControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new AccesoEspecialController(_mediatorMock.Object);
        // Simula el HttpContext para el TraceIdentifier
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    [Fact]
    public async Task ObtenerAccesosEspecialesPaginado_ReturnsOk()
    {
        // Arrange
        var request = new AccesoEspecialPaginadoRequest();
        var paginadoResult = new AccesoEspecialPaginadoResponseDto();
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<ObtenerAccesoEspecialPaginadoQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paginadoResult);

        // Act
        var result = await _controller.ObtenerAccesosEspecialesPaginado(request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ResponseDto<AccesoEspecialPaginadoResponseDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(paginadoResult, response.Data);
    }

    [Fact]
    public async Task RegistrarAccesoEspecial_ReturnsCreated()
    {
        // Arrange
        var request = new RegistrarAccesoEspecialRequest { IdUsuario = 1, IdArea = 2 };
        long expectedId = 123;
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedId);

        // Act
        var result = await _controller.RegistrarAccesoEspecial(request, CancellationToken.None);

        // Assert
        var createdResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
        var response = Assert.IsType<ResponseDto<long>>(createdResult.Value);
        Assert.True(response.Success);
        Assert.Equal(expectedId, response.Data);
    }

    [Fact]
    public async Task EliminarAccesoEspecial_ReturnsOk()
    {
        // Arrange
        long id = 1;
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<EliminarAccesoEspecialCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.EliminarAccesoEspecial(id, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ResponseDto<bool>>(okResult.Value);
        Assert.True(response.Success);
        Assert.True(response.Data);
    }

    [Fact]
    public async Task CambiarEstadoAccesoEspecial_ReturnsOk()
    {
        // Arrange
        long id = 1;
        var request = new CambiarEstadoAccesoEspecialRequest { Estado = true };
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CambiarEstadoAccesoEspecialCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.CambiarEstadoAccesoEspecial(id, request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ResponseDto<bool>>(okResult.Value);
        Assert.True(response.Success);
        Assert.True(response.Data);
    }

    [Fact]
    public async Task ValidarExisteEstadoAccesoEspecial_ReturnsOk()
    {
        // Arrange
        long idUsuario = 1, idArea = 2;
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<ValidarExisteAccesoEspecialQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ValidarExisteEstadoAccesoEspecial(idUsuario, idArea, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ResponseDto<bool>>(okResult.Value);
        Assert.True(response.Success);
        Assert.True(response.Data);
    }
}