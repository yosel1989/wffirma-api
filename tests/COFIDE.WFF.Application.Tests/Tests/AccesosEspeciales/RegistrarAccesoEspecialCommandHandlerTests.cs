using Moq;
using AutoMapper;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Commands.RegistrarAccesoEspecial;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Ports.Out;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Services;
using COFIDE.WFF.Application.Features.AccesosEspeciales.DTOs;
using COFIDE.WFF.Domain.Aggregates.AccesoEspecialAggregate;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Usuarios.Ports.Out;

public class RegistrarAccesoEspecialCommandHandlerTests
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IAccesoEspecialRegistrarService> _serviceMock;
    private readonly Mock<IAccesoEspecialRepository> _repoMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly Mock<IUsuarioRepository> _usuarioRepoMock;
    private readonly RegistrarAccesoEspecialCommandHandler _handler;

    public RegistrarAccesoEspecialCommandHandlerTests()
    {
        _mapperMock = new Mock<IMapper>();
        _serviceMock = new Mock<IAccesoEspecialRegistrarService>();
        _repoMock = new Mock<IAccesoEspecialRepository>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _usuarioRepoMock = new Mock<IUsuarioRepository>();

        _handler = new RegistrarAccesoEspecialCommandHandler(_currentUserMock.Object, _mapperMock.Object, _serviceMock.Object, _repoMock.Object, _usuarioRepoMock.Object);
    }

    [Fact]
    public async Task Handle_RegistroCorrecto_RetornaId()
    {
        // Arrange
        var command = new RegistrarAccesoEspecialCommand(1, 2);
        var dto = new AccesoEspecialRequestDto { IdUsuario = 1, IdArea = 2 };
        var accesoEspecial = AccesoEspecial.Crear(99, 1, 2);

        _mapperMock.Setup(m => m.Map<AccesoEspecialRequestDto>(command)).Returns(dto);
        _serviceMock.Setup(s => s.AccesoEspecialRegistrar(dto, It.IsAny<CancellationToken>())).Returns(accesoEspecial);
        _repoMock.Setup(r => r.AddAsync(accesoEspecial)).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(99, result);
        _mapperMock.Verify(m => m.Map<AccesoEspecialRequestDto>(command), Times.Once);
        _serviceMock.Verify(s => s.AccesoEspecialRegistrar(dto, It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.AddAsync(accesoEspecial), Times.Once);
    }
}