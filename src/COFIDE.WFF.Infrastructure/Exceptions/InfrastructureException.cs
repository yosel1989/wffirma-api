using COFIDE.WFF.Application.Common.Errors;
namespace COFIDE.WFF.Infrastructure.Exceptions;

public sealed class InfrastructureException : InfrastructureExceptionBase
{
    public InfrastructureException(
        ApplicationError error,
        int? httpStatusCode = null,
        Exception? inner = null)
        : base(error, httpStatusCode, inner)
    {
    }
}
