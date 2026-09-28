using COFIDE.WFF.Domain.Common.Entities;
namespace COFIDE.WFF.Domain.Entities;

public class Area : AuditableEntity<long>
{
    public string CodDependencia { get; private set; } = null!;
    public string NombreArea { get; private set; } = null!;
    public string DescripcionCorta { get; private set; } = null!;
    public char Tipo { get; }
    public long? IdAreaPadre { get; }
    public Area AreaPadre { get; private set; } = default!;
    public ICollection<Area> SubAreas { get; private set; } = new List<Area>();
}
