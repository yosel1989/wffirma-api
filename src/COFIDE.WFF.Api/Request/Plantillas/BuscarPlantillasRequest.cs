using COFIDE.WFF.Api.Request.Comun;

namespace COFIDE.WFF.Api.Request.Plantillas
{
    /// <summary>Filtros del listado paginado de plantillas (query string).</summary>
    public sealed class BuscarPlantillasRequest : PaginadoRequest
    {
        public string? Busqueda { get; init; }
        public bool Visibilidad { get; init; }
        public List<long>? Aprobadores { get; init; }
        public List<string>? Gerencias { get; init; }
    }
}
