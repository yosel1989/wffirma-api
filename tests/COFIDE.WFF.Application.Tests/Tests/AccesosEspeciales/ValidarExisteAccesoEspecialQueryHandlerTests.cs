using Moq;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Queries.ValidarExisteAccesoEspecial;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Ports.Out;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Common.Errors;

public class ValidarExisteAccesoEspecialQueryHandlerTests
{
    private readonly Mock<ICurrentUserService> _userMock;
    private readonly Mock<IAccesoEspecialQueryRepository> _repoMock;
    private readonly ValidarExisteAccesoEspecialQueryHandler _handler;

    public ValidarExisteAccesoEspecialQueryHandlerTests()
    {
        _userMock = new Mock<ICurrentUserService>();
        _repoMock = new Mock<IAccesoEspecialQueryRepository>();
        _handler = new ValidarExisteAccesoEspecialQueryHandler(_userMock.Object, _repoMock.Object);
    }

    [Fact]
    public async Task Handle_UsuarioAutenticadoYConCargo_RetornaValorExistencia()
    {
        // Arrange
        _userMock.SetupGet(u => u.UserIdLocal).Returns(10);
        _userMock.SetupGet(u => u.UserCargoId).Returns(20);

        var query = new ValidarExisteAccesoEspecialQuery(1, 2);
        _repoMock.Setup(r => r.ExistAsync(1, 2, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result);
        _repoMock.Verify(r => r.ExistAsync(1, 2, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UsuarioNoAutenticado_LanzaExcepcion()
    {
        // Arrange
        _userMock.SetupGet(u => u.UserIdLocal).Returns((long?)null);
        _userMock.SetupGet(u => u.UserCargoId).Returns(20);

        var query = new ValidarExisteAccesoEspecialQuery(1, 2);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<COFIDE.WFF.Application.Exceptions.ApplicationException>(
            () => _handler.Handle(query, CancellationToken.None));
        Assert.Equal(ApplicationErrors.UserNotAuthenticated.Description, ex.Message);
    }

    [Fact]
    public async Task Handle_UsuarioSinCargo_LanzaExcepcion()
    {
        // Arrange
        _userMock.SetupGet(u => u.UserIdLocal).Returns(10);
        _userMock.SetupGet(u => u.UserCargoId).Returns((long?)null);

        var query = new ValidarExisteAccesoEspecialQuery(1, 2);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<COFIDE.WFF.Application.Exceptions.ApplicationException>(
            () => _handler.Handle(query, CancellationToken.None));
        Assert.Equal(ApplicationErrors.UserNotAuthenticated.Description, ex.Message);
    }
}
