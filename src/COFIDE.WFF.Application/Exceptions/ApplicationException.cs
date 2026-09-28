using COFIDE.WFF.Application.Common.Errors;
namespace COFIDE.WFF.Application.Exceptions;

public sealed class ApplicationException(
        ApplicationError error,
        Exception? inner = null
    ) : ApplicationExceptionBase(error, inner);