namespace COFIDE.WFF.Domain.Common.Errors;

public sealed record DomainError(
    string Code,
    string Description)
{
    public DomainError Format(params object[] args) =>
        this with { Description = string.Format(Description, args) };
}