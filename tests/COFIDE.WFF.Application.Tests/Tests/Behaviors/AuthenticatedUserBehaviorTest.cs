using COFIDE.WFF.Application.Common.Behaviors;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Ports.Out;
using FluentAssertions;
using MediatR;
using Moq;
using applicationException = COFIDE.WFF.Application.Exceptions;
namespace COFIDE.WFF.Application.Tests.Tests.Behaviors;

public class AuthenticatedUserBehaviorTest
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();

    [Fact]
    public async Task Debe_Lanzar_AuthRequired_Cuando_Usuario_No_Esta_Autenticado()
    {
        _currentUserMock
            .Setup(x => x.Autenticado)
            .Returns(false);

        var behavior =
            new AuthenticatedUserBehavior<TestRequest, string>(
                _currentUserMock.Object);

        RequestHandlerDelegate<string> next =
            (ct) => Task.FromResult("OK");

        Func<Task> act = () =>
            behavior.Handle(
                new TestRequest(),
                next,
                CancellationToken.None);

        var exception = await act.Should()
            .ThrowAsync<applicationException.ApplicationException>();

        exception.Which.Error.Code
            .Should()
            .Be(ApplicationErrors.UserNotAuthenticated.Code);
    }

    [Fact]
    public async Task Debe_Lanzar_AccessDenied_Cuando_UserIdLocal_Es_Null()
    {
        _currentUserMock
            .Setup(x => x.Autenticado)
            .Returns(true);

        _currentUserMock
            .Setup(x => x.UserIdLocal)
            .Returns((long?)null);

        var behavior =
            new AuthenticatedUserBehavior<TestRequest, string>(
                _currentUserMock.Object);

        RequestHandlerDelegate<string> next =
            (ct) => Task.FromResult("OK");

        Func<Task> act = () =>
            behavior.Handle(
                new TestRequest(),
                next,
                CancellationToken.None);

        var exception = await act.Should()
            .ThrowAsync<applicationException.ApplicationException>();

        exception.Which.Error.Code
            .Should()
            .Be(ApplicationErrors.AccessDenied.Code);
    }

    [Fact]
    public async Task Debe_Continuar_Cuando_Usuario_Esta_Autenticado()
    {
        _currentUserMock
            .Setup(x => x.Autenticado)
            .Returns(true);

        _currentUserMock
            .Setup(x => x.UserIdLocal)
            .Returns(10);

        var nextExecuted = false;

        RequestHandlerDelegate<string> next =
            (ct) =>
            {
                nextExecuted = true;
                return Task.FromResult("OK");
            };

        var behavior =
            new AuthenticatedUserBehavior<TestRequest, string>(
                _currentUserMock.Object);

        var result = await behavior.Handle(
            new TestRequest(),
            next,
            CancellationToken.None);

        result.Should().Be("OK");

        nextExecuted.Should().BeTrue();
    }

    private sealed class TestRequest { }
}
