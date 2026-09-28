using COFIDE.WFF.Domain.Common.Entities;
namespace COFIDE.WFF.Domain.Entities;

public class EstadoDocumento : AuditableEntity<long>
{
    public string NombreEstadoDocumento { get; } = null!;
    public string? Descripcion { get; }
}
