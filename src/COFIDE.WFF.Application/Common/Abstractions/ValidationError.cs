namespace COFIDE.WFF.Application.Common.Abstractions;

public record ValidationError(
  string Code,
  string Message,
  string Field
);
