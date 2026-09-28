namespace COFIDE.WFF.Application.Common.Commands;

/// <summary>
/// Comando base abstracto para operaciones que requieren trazabilidad del usuario ejecutor.
/// Este patrón permite inyectar el identificador del usuario autenticado y el terminal desde el backend,
/// a través de un comportamiento de pipeline (como AuditingBehavior),
/// evitando así exponer datos de auditoría directamente al cliente.
///
/// Esta clase está diseñada para escenarios donde se desea un control centralizado
/// de la auditoría, sin pasar explícitamente el usuario desde el consumidor externo.
/// </summary>
/// <typeparam name="TResponse">Tipo de dato que retorna el comando al ejecutarse.</typeparam>
public abstract record AuditableCommand<TResponse> : BaseCommand<TResponse>
{
    /// <summary>
    /// Identificador del usuario que ejecuta la acción.
    /// </summary>
    private string? AuditUserName { get; set; }

    /// <summary>
    /// IP o nombre del terminal desde el que se ejecuta la acción.
    /// </summary>
    private string? AuditTerminal { get; set; }

    /// <summary>
    /// Establece la información de auditoría: usuario y terminal.
    /// </summary>
    /// <param name="userName">Nombre del usuario autenticado.</param>
    /// <param name="terminal">IP o nombre del equipo origen.</param>
    public void SetAuditContext(string? userName, string? terminal)
    {
        SetAuditUser(userName);
        SetAuditTerminal(terminal);
    }

    /// <summary>
    /// Establece el nombre del usuario desde el pipeline.
    /// </summary>
    /// <param name="userName">Nombre del usuario autenticado.</param>
    public void SetAuditUser(string? userName)
    {
        if (!string.IsNullOrWhiteSpace(userName))
            AuditUserName = userName;
    }

    /// <summary>
    /// Establece el nombre del Terminal desde el pipeline.
    /// </summary>
    /// <param name="userTerminal">Nombre del Terminal autenticado.</param>
    public void SetAuditTerminal(string? userTerminal)
    {
        if (!string.IsNullOrWhiteSpace(userTerminal))
            AuditTerminal = userTerminal;
    }
}
