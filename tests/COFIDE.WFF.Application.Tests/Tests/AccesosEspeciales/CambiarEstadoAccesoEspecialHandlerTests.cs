using Moq;
using AutoMapper;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Commands.CambiarEstadoAccesoEspecial;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.AccesoEspecialAggregate;

public class CambiarEstadoAccesoEspecialHandlerTests
{
    private readonly Mock<IAccesoEspecialRepository> _repoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly CambiarEstadoAccesoEspecialHandler _handler;

    public CambiarEstadoAccesoEspecialHandlerTests()
    {
        _repoMock = new Mock<IAccesoEspecialRepository>();
        _mapperMock = new Mock<IMapper>();
        _handler = new CambiarEstadoAccesoEspecialHandler(_mapperMock.Object, _repoMock.Object);
    }

    [Fact]
    public async Task Handle_AccesoEspecialNoExiste_ReturnsFalse()
    {
        // Arrange
        var command = new CambiarEstadoAccesoEspecialCommand(1, true);
        _repoMock.Setup(r => r.GetByIdAnyStatusAsync(1, true)).ReturnsAsync((AccesoEspecial?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result);
        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<AccesoEspecial>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ActivaAccesoEspecial_LlamaActivarYActualiza()
    {
        // Arrange
        var command = new CambiarEstadoAccesoEspecialCommand(1, true);
        var accesoEspecial = new Mock<AccesoEspecial>();
        _repoMock.Setup(r => r.GetByIdAnyStatusAsync(1, true)).ReturnsAsync(accesoEspecial.Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        accesoEspecial.Verify(a => a.Activar(), Times.Once);
        accesoEspecial.Verify(a => a.Desactivar(), Times.Never);
        _repoMock.Verify(r => r.UpdateAsync(accesoEspecial.Object), Times.Once);
    }

    [Fact]
    public async Task Handle_DesactivaAccesoEspecial_LlamaDesactivarYActualiza()
    {
        // Arrange
        var command = new CambiarEstadoAccesoEspecialCommand(1, false);
        var accesoEspecial = new Mock<AccesoEspecial>();
        _repoMock.Setup(r => r.GetByIdAnyStatusAsync(1, true)).ReturnsAsync(accesoEspecial.Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        accesoEspecial.Verify(a => a.Activar(), Times.Never);
        accesoEspecial.Verify(a => a.Desactivar(), Times.Once);
        _repoMock.Verify(r => r.UpdateAsync(accesoEspecial.Object), Times.Once);
    }
}