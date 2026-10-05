namespace COFIDE.WFF.Api.Request.Solicitudes
{
    public sealed class SolicitudFormRequest
    {
        public long IdTipoSolicitud { get; init; }
        public long IdSubTipoSolicitud { get; init; }
        public long IdtipoOperacion { get; init; }
        public string? Asunto { get; init; }
        public string? Mensaje { get; init; }
        public bool EsUrgente { get; init; }
        public bool InCodigo { get; init; }
        public DateTime? FechaLimite { get; init; }
        public decimal? PesoTotalDocu { get; init; }
        public List<IFormFile>? Documentos { get; init; }
        public List<IFormFile>? Sustentos { get; init; }
        public List<long>? IdsDocumentosConservar { get; init; }
        public List<long>? IdsSustentosConservar { get; init; }
        public List<string>? CodigosReferencia { get; init; }
        public long? IdPlantilla { get; init; }
        public string? Configuracion { get; init; }
        public List<string>? Correos { get; init; }
        public string? Tags { get; init; }
    }

}
