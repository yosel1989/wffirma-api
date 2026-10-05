using COFIDE.WFF.Domain.Common.Entities;
namespace COFIDE.WFF.Domain.Entities;

public class TipoOperacion : AuditableEntity<long>
{
    public string NombreTipoOperacion { get; private set; } = null!;
    public long IdArea { get; private set; }
    public Area Area { get; private set; } = default!;
}
