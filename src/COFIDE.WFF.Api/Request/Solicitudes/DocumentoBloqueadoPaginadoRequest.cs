using COFIDE.WFF.Api.Request.Comun;

namespace COFIDE.WFF.Api.Request.Solicitudes
{
    /// <summary>Filtros del listado de documentos bloqueados (query string).</summary>
    public sealed class DocumentoBloqueadoPaginadoRequest : PaginadoRequest
    {
        public string? FechaInicio { get; init; }
        public string? FechaFin { get; init; }
        public string? NombreArchivo { get; init; }
        public string? CodigoSolicitud { get; init; }
        public List<long>? FirmanteActual { get; init; }
    }
}
