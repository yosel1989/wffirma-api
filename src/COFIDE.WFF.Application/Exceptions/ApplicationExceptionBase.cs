using COFIDE.WFF.Application.Common.Errors;
namespace COFIDE.WFF.Application.Exceptions;

public abstract class ApplicationExceptionBase(
        ApplicationError error,
        Exception? inner = null
    ) : Exception(error.Description, inner)
{
    public ApplicationError Error { get; } = error;
}