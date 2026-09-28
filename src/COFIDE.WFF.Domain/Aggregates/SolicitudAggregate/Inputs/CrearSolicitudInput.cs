namespace COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Inputs;

public class CrearSolicitudInput
{
    public string CodigoSolicitud { get; init; } = default!;
    public long IdTipoSolicitud { get; init; }
    public long IdSubTipoSolicitud { get; init; }
    public long IdEstadoSolicitud { get; init; }
    public long IdUsuarioSolicitante { get; init; }
    public DateTime FechaSolicitud { get; init; }
    public long IdTipoOperacion { get; init; }
    public long IdUsuarioAccion { get; init; }
    public long IdAreaSolicitud { get; init; }
    public long IdAreaPadreSolicitud { get; init; }
    public DateTime FechaAccion { get; init; }
    public string? Asunto { get; init; }
    public string? Mensaje { get; init; }
    public bool EsUrgente { get; init; }
    public bool InCodigo { get; init; }
    public DateTime? FechaLimite { get; init; }
    public string? MotivoRechazo { get; init; }
    public decimal? PesoTotalDocu { get; init; }
    public List<string> CodigosReferencia { get; init; } = default!;
    public List<string> Correos { get; init; } = default!;
    public List<DocumentoProcesadoInput> Documentos { get; init; } = default!;
    public List<SustentoProcesadoInput> Sustentos { get; init; } = default!;
    public int Anio { get; init; }
    public string CodigoArea { get; init; } = default!;
    public string CodigoTipoSolicitud { get; init; } = default!;
    public long CorrelativoCodigoSolicitud { get; init; }
}
