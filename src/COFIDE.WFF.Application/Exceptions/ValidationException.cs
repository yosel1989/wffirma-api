using COFIDE.WFF.Application.Common.Errors;
using FluentValidation.Results;
namespace COFIDE.WFF.Application.Exceptions;
public sealed class ValidationException : ApplicationExceptionBase
{
    public IReadOnlyCollection<ValidationError> Errors { get; }

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : base(ApplicationErrors.ValidationFailed)
    {
        Errors = failures
            .Select(f => new ValidationError(
                f.ErrorCode ?? "VALIDATION_ERROR",
                f.ErrorMessage,
                f.PropertyName))
            .ToList();
    }

    public ValidationException(IReadOnlyCollection<ValidationError> errors)
        : base(ApplicationErrors.ValidationFailed)
    {
        Errors = errors;
    }
}

public sealed record ValidationError(
    string Code,
    string Message,
    string Property);