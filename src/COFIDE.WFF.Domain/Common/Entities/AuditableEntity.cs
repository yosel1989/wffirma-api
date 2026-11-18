using COFIDE.WFF.Domain.Common.Abstractions;

using COFIDE.WFF.Domain.Common.Tiempo;
namespace COFIDE.WFF.Domain.Common.Entities;

public abstract class AuditableEntity<TKey> : BaseEntity<TKey>, IAuditableEntity<TKey> where TKey : struct
{
    /// <summary>
    /// Obtiene o establece el estado lógico activo o inactivo.
    /// </summary>
    public virtual bool Estado { get; set; }
    // Auditorísa de creación
    public DateTime FechaCreacion { get; private set; } = HoraSistema.Ahora;
    public string UsuarioCreacion { get; private set; } = string.Empty;
    public string TerminalCreacion { get; private set; } = string.Empty;

    // Auditoría de modificación
    public DateTime? FechaModificacion { get; private set; }
    public string? UsuarioModificacion { get; private set; }
    public string? TerminalModificacion { get; private set; }

    public void SetCreationAudit(string nombreUsuario, string terminalUsuario)
    {
        UsuarioCreacion = nombreUsuario;
        FechaCreacion = HoraSistema.Ahora;
        TerminalCreacion = terminalUsuario;
    }

    public void SetUpdateAudit(string nombreUsuario, string terminalUsuario)
    {
        UsuarioModificacion = nombreUsuario;
        FechaModificacion = HoraSistema.Ahora;
        TerminalModificacion = terminalUsuario;
    }
    /// <summary>
    /// Activa la entidad.
    /// </summary>
    public virtual void Activar() => Estado = true;

    /// <summary>
    /// Desactiva la entidad.
    /// </summary>
    public virtual void Desactivar() => Estado = false;
}
