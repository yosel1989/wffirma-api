using COFIDE.WFF.Application.Common.Abstractions;
namespace COFIDE.WFF.Application.Common.Queries;

public abstract record BaseQuery<TResult> : IQuery<TResult> { }
