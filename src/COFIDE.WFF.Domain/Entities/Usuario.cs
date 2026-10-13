using COFIDE.WFF.Domain.Common.Entities;
using COFIDE.WFF.Domain.Common.Enums;
namespace COFIDE.WFF.Domain.Entities;

public class Usuario : AuditableEntity<long>
{
    public string Nombres { get; private set; } = null!;
    public string ApellidoPaterno { get; private set; } = null!;
    public string ApellidoMaterno { get; private set; } = null!;
    public string NumeroDocumento { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string CuentaLogueo { get; private set; } = null!;
    public long? IdArea { get; }
    public long? IdCargo { get; }
    public bool? Notificacion { get; }
    public Area Area { get; private set; } = default!;
    public Cargo Cargo { get; private set; } = default!;

    public RolUsuario Rol { get; }

    private Usuario() { }

    public Usuario(long id, RolUsuario rol)
    {
        Id = id;
        Rol = rol;
    }

    public bool EsAdministrador()
        => Rol == RolUsuario.Administrador;

    public bool EsCoordinador()
        => Rol == RolUsuario.Coordinador;

    public bool EsSolicitante()
        => Rol == RolUsuario.SolicitanteAprobador;
}
