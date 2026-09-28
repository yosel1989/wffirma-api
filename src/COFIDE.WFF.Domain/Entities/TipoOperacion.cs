using COFIDE.WFF.Domain.Common.Entities;
namespace COFIDE.WFF.Domain.Entities;

public class TipoOperacion : AuditableEntity<long>
{
    public string NombreTipoOperacion { get; set; } = null!;
    public long IdArea { get; set; }
    public Area Area { get; set; } = default!;
}
