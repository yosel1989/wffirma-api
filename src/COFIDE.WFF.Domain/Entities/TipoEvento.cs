using COFIDE.WFF.Domain.Common.Entities;
namespace COFIDE.WFF.Domain.Entities;

public class TipoEvento : AuditableEntity<long>
{
    public string NombreEvento { get; private set; } = null!;
    public string? Descripcion { get; }
    public string? FormatoMensaje { get; }
    public bool? RequiereDestino { get; }
}
