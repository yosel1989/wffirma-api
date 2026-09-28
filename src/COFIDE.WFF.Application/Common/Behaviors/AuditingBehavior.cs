using COFIDE.WFF.Application.Common.Commands;
using COFIDE.WFF.Application.Common.Ports.Out;
using MediatR;
namespace COFIDE.WFF.Application.Common.Behaviors;

public class AuditingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
     where TRequest : IRequest<TResponse>
{
    private readonly ICurrentUserService _currentUser;

    public AuditingBehavior(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Aplicar auditoría solo si el request lo soporta
        if (request is AuditableCommand<TResponse> auditable)
        {
            var userName = _currentUser.CodigoUsuario;
            var terminal = _currentUser.IpAddress;

            auditable.SetAuditContext(userName, terminal);
        }

        var response = await next(cancellationToken);

        return response;
    }
}
