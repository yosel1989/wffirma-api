using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Usuarios.Commands.CambiarEstadoUsuario;
using COFIDE.WFF.Application.Features.Usuarios.Commands.CambiarEstadoUsuario.Errors;
using COFIDE.WFF.Application.Features.Usuarios.Ports.Out;
using COFIDE.WFF.Domain.Common.Enums;
using COFIDE.WFF.Domain.Entities;
using Moq;
using ApplicationException = COFIDE.WFF.Application.Exceptions.ApplicationException;

namespace COFIDE.WFF.Application.Tests.Tests.Usuarios;

public sealed class CambiarEstadoUsuarioHandlerTest
{
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IUsuarioRepository> _repository = new(MockBehavior.Strict);

    public CambiarEstadoUsuarioHandlerTest()
    {
        _currentUser.SetupGet(x => x.Autenticado).Returns(true);
        _currentUser.SetupGet(x => x.UserIdLocal).Returns(100L);
        _currentUser.SetupGet(x => x.UserCargoId).Returns(10L);
        _currentUser.SetupGet(x => x.CodigoRolUsuario).Returns(RolUsuarioCodigo.Administrador);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public async Task Handle_EstadoSolicitado_ActualizaYDevuelveEstadoFinal(bool inicial, bool solicitado)
    {
        var usuario = new Usuario(7, RolUsuario.SolicitanteAprobador) { Estado = inicial };
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        _repository.Setup(x => x.GetByIdAnyStatusAsync(7, true, token)).ReturnsAsync(usuario);
        _repository.Setup(x => x.UpdateAsync(usuario)).Returns(Task.CompletedTask);
        var handler = new CambiarEstadoUsuarioHandler(_currentUser.Object, _repository.Object);

        var result = await handler.Handle(new(7, solicitado), token);

        Assert.Equal(solicitado, result);
        Assert.Equal(solicitado, usuario.Estado);
        _repository.Verify(x => x.GetByIdAnyStatusAsync(7, true, token), Times.Once);
        _repository.Verify(x => x.UpdateAsync(usuario), Times.Once);
        _repository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_UsuarioInexistente_Lanza404SinActualizar()
    {
        _repository.Setup(x => x.GetByIdAnyStatusAsync(7, true, default)).ReturnsAsync((Usuario?)null);
        var handler = new CambiarEstadoUsuarioHandler(_currentUser.Object, _repository.Object);

        var error = await Assert.ThrowsAsync<ApplicationException>(() => handler.Handle(new(7, true), default));

        Assert.Equal(CambiarEstadoUsuarioErrors.UsuarioNoExiste, error.Error);
        Assert.Equal(404, error.HttpStatusCode);
        _repository.Verify(x => x.GetByIdAnyStatusAsync(7, true, default), Times.Once);
        _repository.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(false, 100L, 10L)]
    [InlineData(true, null, 10L)]
    [InlineData(true, 100L, null)]
    public async Task Handle_ContextoNoAutenticado_Lanza401SinConsultar(bool autenticado, long? id, long? cargo)
    {
        _currentUser.SetupGet(x => x.Autenticado).Returns(autenticado);
        _currentUser.SetupGet(x => x.UserIdLocal).Returns(id);
        _currentUser.SetupGet(x => x.UserCargoId).Returns(cargo);
        var handler = new CambiarEstadoUsuarioHandler(_currentUser.Object, _repository.Object);

        var error = await Assert.ThrowsAsync<ApplicationException>(() => handler.Handle(new(7, false), default));

        Assert.Equal(ApplicationErrors.UserNotAuthenticated.Code, error.Error.Code);
        Assert.Equal(401, error.HttpStatusCode);
        _repository.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(RolUsuarioCodigo.Coordinador)]
    [InlineData(RolUsuarioCodigo.SolicitanteAprobador)]
    [InlineData("DESCONOCIDO")]
    [InlineData(null)]
    public async Task Handle_RolNoAdministrador_Lanza403SinConsultar(string? rol)
    {
        _currentUser.SetupGet(x => x.CodigoRolUsuario).Returns(rol);
        var handler = new CambiarEstadoUsuarioHandler(_currentUser.Object, _repository.Object);

        var error = await Assert.ThrowsAsync<ApplicationException>(() => handler.Handle(new(7, true), default));

        Assert.Equal(ApplicationErrors.Forbidden.Code, error.Error.Code);
        Assert.Equal(403, error.HttpStatusCode);
        _repository.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(-1L, true, false)]
    [InlineData(0L, false, false)]
    [InlineData(1L, true, true)]
    [InlineData(1L, false, true)]
    public void Validator_IdYEstado_ValidaIdentificadorSinRechazarInactivo(long id, bool estado, bool valido)
    {
        var result = new CambiarEstadoUsuarioValidator().Validate(new CambiarEstadoUsuarioCommand(id, estado));

        Assert.Equal(valido, result.IsValid);
        if (!valido)
        {
            var error = Assert.Single(result.Errors);
            Assert.Equal(nameof(CambiarEstadoUsuarioCommand.Id), error.PropertyName);
            Assert.Equal("GreaterThanValidator", error.ErrorCode);
        }
        else
            Assert.Empty(result.Errors);
    }
}
