using COFIDE.WFF.Application.Common.Abstractions;

namespace COFIDE.WFF.Application.Common.Commands;

/// <summary>
/// Comando base para eliminaciones físicas. No es auditable: no inyecta UserId.
/// </summary>
public abstract record DeleteCommand<TResponse, TKey>(TKey Id)
    : ICommand<TResponse>;