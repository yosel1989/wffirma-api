using Moq;
using AutoMapper;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Commands.EliminarAccesoEspecial;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.AccesoEspecialAggregate;
using COFIDE.WFF.Application.Common.Ports.Out;

public class EliminarAccesoEspecialCommandHandlerTests
{
    private readonly Mock<IAccesoEspecialRepository> _repoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly EliminarAccesoEspecialCommandHandler _handler;

    public EliminarAccesoEspecialCommandHandlerTests()
    {
        _repoMock = new Mock<IAccesoEspecialRepository>();
        _mapperMock = new Mock<IMapper>();
        _currentUserMock = new Mock<ICurrentUserService>();

        _handler = new EliminarAccesoEspecialCommandHandler(_currentUserMock.Object, _mapperMock.Object, _repoMock.Object);
    }

    [Fact]
    public async Task Handle_AccesoEspecialNoExiste_ReturnsFalse()
    {
        // Arrange
        var command = new EliminarAccesoEspecialCommand(1);
        _repoMock.Setup(r => r.GetByIdAsync(1, true)).ReturnsAsync((AccesoEspecial?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result);
        _repoMock.Verify(r => r.Delete(It.IsAny<AccesoEspecial>()), Times.Never);
    }

    [Fact]
    public async Task Handle_AccesoEspecialExiste_EliminaYRetornaTrue()
    {
        // Arrange
        var command = new EliminarAccesoEspecialCommand(1);
        var accesoEspecial = AccesoEspecial.Crear(1, 2, 3);
        _repoMock.Setup(r => r.GetByIdAsync(1, true)).ReturnsAsync(accesoEspecial);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        _repoMock.Verify(r => r.Delete(accesoEspecial), Times.Once);
    }
}