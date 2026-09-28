using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Inputs;
namespace COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Models;

public class SolicitudEditModel
{
    public long Id { get; set; }
    public long IdTipoSolicitud { get; set; }
    public long IdSubTipoSolicitud { get; set; }
    public string? Asunto { get; set; }
    public string? Mensaje { get; set; }
    public bool EsUrgente { get; set; }
    public bool InCodigo { get; set; }
    public DateTime? FechaLimite { get; set; }
    public List<long> DocumentoIdsConservar { get; set; } = [];
    public List<long> SustentoIdsConservar { get; set; } = [];

    public List<DocumentoProcesadoInput> NuevosDocumentosRequest { get; init; } = default!;
    public List<SustentoProcesadoInput> NuevosSustentosRequest { get; init; } = default!;
    public List<string> CodigosReferencia { get; init; } = default!;
    public List<string> Correos { get; init; } = default!;
}