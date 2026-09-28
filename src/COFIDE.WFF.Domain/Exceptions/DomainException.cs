using COFIDE.WFF.Domain.Common.Errors;
namespace COFIDE.WFF.Domain.Exceptions;

public sealed class DomainException : DomainExceptionBase
{
    public DomainException(
        DomainError error,
        Exception? inner = null)
        : base(error, inner)
    {
    }
}