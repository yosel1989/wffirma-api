namespace COFIDE.WFF.Application.Common.Commands;

/// <summary>
/// Comando base para operaciones de actualización con auditoría y validación.
/// Hereda de <see cref="AuditableCommand{TResponse}"/> para inyección de UserId,
/// e implementa <see cref="IValidatable"/> para activar validaciones automáticas.
/// </summary>
/// <typeparam name="TResponse">Tipo de respuesta esperada (por ejemplo, bool o Guid).</typeparam>
/// <typeparam name="TKey">Tipo de clave primaria de la entidad a actualizar.</typeparam>
public abstract record UpdateCommand<TResponse, TKey>(
    TKey Id
) : AuditableCommand<TResponse>;
