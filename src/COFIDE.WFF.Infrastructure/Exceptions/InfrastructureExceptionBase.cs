using COFIDE.WFF.Application.Common.Errors;
namespace COFIDE.WFF.Infrastructure.Exceptions;

public abstract class InfrastructureExceptionBase : Exception
{
    public ApplicationError Error { get; }
    public int? HttpStatusCode { get; }

    protected InfrastructureExceptionBase(
        ApplicationError error,
        int? httpStatusCode = null,
        Exception? inner = null)
        : base(error.Description, inner)
    {
        Error = error;
        HttpStatusCode = httpStatusCode;
    }
}