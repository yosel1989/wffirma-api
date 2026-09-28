using COFIDE.WFF.Domain.Common.Entities;
namespace COFIDE.WFF.Domain.Entities;
public class TipoSolicitud : AuditableEntity<long>
{
    public string NombreTipoSolicitud { get; set; } = null!;
    public string DescripcionCorta { get; private set; } = null!;
    public long IdArea { get; set; }
    public Area Area { get; set; } = default!;
}
