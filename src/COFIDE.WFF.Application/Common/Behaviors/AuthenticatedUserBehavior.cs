using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Interfaces;
using COFIDE.WFF.Application.Common.Ports.Out;
using MediatR;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;

namespace COFIDE.WFF.Application.Common.Behaviors;

public class AuthenticatedUserBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly ICurrentUserService _currentUser;

    public AuthenticatedUserBehavior(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is IAllowAnonymousRequest)
            return await next(cancellationToken);

        if (!_currentUser.Autenticado)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.UserNotAuthenticated);

        if (_currentUser.UserIdLocal is null)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.AccessDenied);

        return await next(cancellationToken);
    }
}
