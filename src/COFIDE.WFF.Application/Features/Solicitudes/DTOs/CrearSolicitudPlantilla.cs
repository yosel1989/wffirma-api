namespace COFIDE.WFF.Application.Features.Solicitudes.DTOs;
public class CrearSolicitudPlantilla
{
    public long? Id { get; set; }
    public string? NombrePlantilla { get; set; }
    public string? Descripcion { get; set; }
    public bool? Visibilidad { get; set; }
    public bool? EsReutilizable { get; set; }
    public List<PlantillaEtapaRequest>? Etapas { get; set; }
}

public class PlantillaEtapaRequest
{
    public long? Id { get; set; }
    public Guid TempId{ get; set; }
    public long OrdenEtapa { get; set; }
    public string NombreEtapa { get; set; } = null!;
    public long IdTipoEtapa { get; set; }
    public long IdTipoAprobador { get; set; }
    public List<FirmanteFlujoRequest>? Firmantes { get; set; }
}

public class FirmanteFlujoRequest
{
    public long? Id { get; set; }
    public long IdUsuarioAsignado { get; set; }
    public string? CargoUsuario { get; set; }
    public string? AreaUsuario { get; set; }
    public string? GerenciaUsuario { get; set; }
    public long IdUsuarioOrigen { get; set; }
}