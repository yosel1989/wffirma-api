namespace COFIDE.WFF.Application.Common.Errors;

public sealed record ApplicationError(
    string Code,
    string Description,
    int? HttpStatusCode = null)
{
    public ApplicationError Format(params object[] args) =>
        this with { Description = string.Format(Description, args) };
}