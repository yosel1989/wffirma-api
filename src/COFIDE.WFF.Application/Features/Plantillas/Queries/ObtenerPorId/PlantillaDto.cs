namespace COFIDE.WFF.Application.Features.Plantillas.Queries.ObtenerPorId;

public class PlantillaDto
{
    public long IdPlantilla { get; init; }
    public string? NombrePlantilla { get; init; }
    public string? Descripcion { get; init; } = null!;
    public bool? Visibilidad { get; init; } = null!;
    public List<PlantillaEtapaDto> Etapas { get; init; } = null!;
}

public class PlantillaEtapaDto
{
    public long Id { get; init; }
    public long OrdenEtapa { get; init; }
    public string NombreEtapa { get; init; } = null!;
    public long IdTipoEtapa { get; init; }
    public string ValorTipoEtapa { get; init; } = null!;
    public long IdTipoAprobador { get; init; }
    public string ValorTipoAprobador { get; init; } = null!;
    public List<FirmanteFlujoDto> Firmantes { get; init; } = new();
}

public class FirmanteFlujoDto
{
    public long Id { get; init; }
    public long IdPlantillaEtapa { get; init; }
    public long IdUsuarioAsignado { get; init; }
    public string? NombreUsuarioAsignado { get; init; }
    public string? Nombres { get; init; }
    public string? ApellidoPaterno { get; init; }
    public string? ApellidoMaterno { get; init; }
    public string? CargoUsuario { get; init; }
    public string? AreaUsuario { get; init; }
    public string? GerenciaUsuario { get; init; }
    public string Correo { get; init; } = null!;
    public bool EsSuplente { get; init; }
    public bool EsDelegado { get; init; }
}
