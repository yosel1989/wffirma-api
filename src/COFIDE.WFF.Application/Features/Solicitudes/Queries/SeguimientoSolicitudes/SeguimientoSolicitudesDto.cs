using System.Text.Json.Serialization;

namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.SeguimientoSolicitudes;

public class SeguimientoSolicitudesResumenDto
{
    public long Total { get; set; }
    public long EnCurso { get; set; }
    public long Aprobado { get; set; }
    public long Rechazado { get; set; }
}


public class SeguimientoSolicitudesDto
{
    public long IdSolicitud { get; set; }
    public string? CodigoSolicitud { get; set; }
    public string? TipoSolicitud { get; set; }
    public string? SubTipoSolicitud { get; set; }
    public string? Urgente { get; set; }
    public string? Estado { get; set; }
    public DateTime? FechaCreacion { get; set; }
    public List<FirmantesDto>? Firmantes { get; set; }
}

public class FirmantesDto
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Nombre { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Estado { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<FirmantesParaleloDto>? Paralelo { get; set; }

    [JsonIgnore]
    public long OrdenEtapa { get; set; }

    [JsonIgnore]
    public string? CodigoTipoEtapa { get; set; }
}

public class FirmantesParaleloDto
{
    public string? Nombre { get; set; }

    public string? Estado { get; set; }
}


public class SeguimientoSolicitudesResponseDto
{
    public long Total { get; set; }
    public long EnCurso { get; set; }
    public long Aprobado { get; set; }
    public long Rechazado { get; set; }
    public IEnumerable<SeguimientoSolicitudesDto> ListadoSolicitudes { get; set; } = [];
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;
}