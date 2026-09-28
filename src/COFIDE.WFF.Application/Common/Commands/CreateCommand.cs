namespace COFIDE.WFF.Application.Common.Commands;

/// <summary>
/// Comando base para operaciones de creación con soporte de auditoría.
/// Hereda de <see cref="AuditableCommand{TResponse}"/> para que el userId
/// se inyecte automáticamente desde el pipeline.
/// </summary>
/// <typeparam name="TResponse">Tipo de respuesta (por ejemplo, el Id de la entidad creada).</typeparam>

public abstract record CreateCommand<TResponse> : AuditableCommand<TResponse>
{}
