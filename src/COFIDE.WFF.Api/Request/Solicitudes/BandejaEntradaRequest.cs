using COFIDE.WFF.Api.Request.Comun;

namespace COFIDE.WFF.Api.Request.Solicitudes
{
    /// <summary>Filtros de la bandeja de entrada del aprobador (query string).</summary>
    public sealed class BandejaEntradaRequest : PaginadoRequest
    {
        public string? Fechainicio { get; init; }
        public string? Fechafin { get; init; }
        public List<long>? Gerencia { get; init; }
        public List<long>? Departamento { get; init; }
        public List<long>? Solicitante { get; init; }
        public List<long>? Tiposolicitud { get; init; }
        public List<long>? Subtiposolicitud { get; init; }
        public List<long>? TipoOperacion { get; init; }
        public string? Codigosolicitud { get; init; }
        public string? Asunto { get; init; }
        public string? Referencia { get; init; }
        public bool? Urgente { get; init; }
        public bool? IncluirResumen { get; init; }
    }
}
