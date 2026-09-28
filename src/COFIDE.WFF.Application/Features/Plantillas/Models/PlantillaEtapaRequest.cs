namespace COFIDE.WFF.Application.Features.Plantillas.Models;
public class PlantillaEtapaRequest
{
    public long? Id { get; set; }
    public long OrdenEtapa { get; set; }
    public string NombreEtapa { get; set; } = null!;
    public long IdTipoEtapa { get; set; }
    public long IdTipoAprobador { get; set; }
    public List<FirmanteFlujoRequest>? Firmantes { get; set; }
}
