namespace COFIDE.WFF.Domain.Common.Abstractions;

/// <summary>
/// Representa una entidad auditable con control de creación, actualización y borrado lógico.
/// </summary>
/// <typeparam name="TKey">Tipo de clave primaria, usualmente Guid o int.</typeparam>
public interface IAuditableEntity<TKey> : IEntity<TKey> where TKey : struct
{
    bool Estado { get; set; }
    // Auditoría de creación
    DateTime FechaCreacion { get; }
    string UsuarioCreacion { get; }
    string TerminalCreacion { get; }

    // Auditoría de modificación
    DateTime? FechaModificacion { get; }
    string? UsuarioModificacion { get; }
    string? TerminalModificacion { get; }

    // Métodos de auditoría
    void SetCreationAudit(string nombreUsuario, string terminalUsuario);
    void SetUpdateAudit(string nombreUsuario, string terminalUsuario);
}