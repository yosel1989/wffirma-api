using COFIDE.WFF.Domain.Common.Entities;
namespace COFIDE.WFF.Domain.Entities;

public class Cargo : AuditableEntity<long>
{
    public string? CodigoCargo { get; } 
    public string NombreCargo { get; private set; } = null!;
}
