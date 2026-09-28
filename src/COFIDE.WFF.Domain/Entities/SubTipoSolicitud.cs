using COFIDE.WFF.Domain.Common.Entities;
namespace COFIDE.WFF.Domain.Entities;
public class SubTipoSolicitud : AuditableEntity<long>
{
    public long IdTipoSolicitud { get; }
    public string NombreSubTipoSolicitud { get; } = null!;
    public long IdArea { get; }
    public TipoSolicitud TipoSolicitud { get; private set; } = default!;
    public Area? Area { get; }
}
