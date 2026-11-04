using COFIDE.WFF.Api.Request.Comun;

namespace COFIDE.WFF.Api.Request.Plantillas;

/// <summary>Filtros del listado de gestión de plantillas (query string).</summary>
public sealed class GestionPlantillasRequest : PaginadoRequest
{
    public string? Fechainicio { get; init; }
    public string? Fechafin { get; init; }
    public List<long>? Gerencia { get; init; }
    public List<long>? Departamento { get; init; }
    public List<long>? Creador { get; init; }
    public List<long>? Aprobador { get; init; }
    public string? NombrePlantilla { get; init; }
}
