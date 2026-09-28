using System.Text.Json.Serialization;

namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerSolicitud;

public class SolicitudDto
{
    public long IdSolicitud { get; set; }
    public long IdTipoSolicitud { get; set; }
    public long IdSubTipoSolicitud { get; set; }
    public long IdtipoOperacion { get; set; }
    public long? IdEstado { get; set; }
    public string? Solicitante { get; set; }
    public string? CodigoSolicitud { get; set; }
    public string? Asunto { get; set; }
    public string? Mensaje { get; set; }
    public bool EsUrgente { get; set; }
    public bool InCodigo { get; set; }
    public string? Estado { get; set; }
    public DateTime? FechaLimite { get; set; }
    public DateTime? FechaCreacion { get; set; }
    public string? MotivoRechazo { get; set; }

    public List<ArchivoDto> Documentos { get; set; } = new();
    public List<SustentoDto> Sustentos { get; set; } = new();
    public List<ComentarioDto> Comentario { get; set; } = new();
    public EstadoFlujov2Dto EstadoFlujo { get; set; } = new();
    public List<HistorialCambiosDto> HistorialCambios { get; set; } = new();
    public List<CodigosReferenciaDto> CodigosReferencia { get; set; } = new();
    public List<CorreoDto> SolicitudCorreo { get; set; } = new();
    public List<HistorialCambiosDto> HistorialEventos { get; set; } = new();
}

public class ArchivoDto
{
    public long Id { get; set; }
    public long? IdDetalleFirma { get; set; }
    public string? Nombre { get; set; }
    public string? Extension { get; set; }
    public string? RutaArchivo { get; set; }
    public long? Peso { get; set; }
    [JsonIgnore]
    public string? NombreOriginal { get; set; }
    [JsonIgnore]
    public string? ExtensionOriginal { get; set; }
    [JsonIgnore]
    public string? RutaOriginal { get; set; }
    [JsonIgnore]
    public string? NombreCodigo { get; set; }
    [JsonIgnore]
    public string? ExtensionCodigo { get; set; }
    [JsonIgnore]
    public string? RutaCodigo { get; set; }
    [JsonIgnore]
    public string? NombreConTag { get; set; }
    [JsonIgnore]
    public string? ExtensionConTag { get; set; }
    [JsonIgnore]
    public string? RutaConTag { get; set; }
    [JsonIgnore]
    public bool InCodigo { get; set; }

    public bool Editar { get; set; }
    public bool FirmadoPorMi { get; set; }
    public bool FirmadoPorOtro { get; set; }
    public long? IdTipoAprobador { get; set; }
    public bool Firmado { get; set; }
    public List<TagsDto> Tags { get; init; } = new();
}

public class SustentoDto
{
    public long Id { get; set; }
    public string Nombre { get; set; } = default!;
    public long? Peso { get; set; }
    public string Extension { get; set; } = default!;
}

public class ComentarioDto
{
    public string? Nombre { get; set; }
    public string? Comentario { get; set; }
    public string? fecha { get; set; }
}

public class DetalleEtapaDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Etapa { get; set; } = string.Empty;
    public bool Estado { get; set; }
}


public class EstadoFlujov2Dto
{
    public long IdPlantilla { get; init; }
    public string? NombrePlantilla { get; init; }
    public string? Descripcion { get; init; } = null!;
    public List<PlantillaEtapav2Dto> etapas { get; init; } = null!;
}

public class PlantillaEtapav2Dto
{
    public long Id { get; init; }
    public long OrdenEtapa { get; init; }
    public string NombreEtapa { get; init; } = null!;
    public long IdTipoEtapa { get; init; }
    public string ValorTipoEtapa { get; init; } = null!;
    public long IdTipoAprobador { get; init; }
    public string ValorTipoAprobador { get; init; } = null!;
    public List<FirmanteFlujov2Dto> Firmantes { get; init; } = new();
}

public class FirmanteFlujov2Dto
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
    public string? DepartamentoUsuario { get; init; }
    public string? GerenciaUsuario { get; init; }
    public string Estado { get; init; } = null!;
    public bool Firmado { get; init; }
    public bool Eliminar { get; init; }
}

public class HistorialCambiosDto
{
    public string? Nombre { get; set; }
    public string? Comentario { get; set; }
    public string? fecha { get; set; }
}

public class CodigosReferenciaDto
{
    public long Id { get; set; }
    public string? Nombre { get; set; }
}

public class CorreoDto
{
    public long Id { get; set; }
    public string? Nombre { get; set; }
}

public class TagsDto
{
    public long Id { get; set; }
    public int NumeroPagina { get; set; }
    public decimal X { get; set; }
    public decimal Y { get; set; }
}
