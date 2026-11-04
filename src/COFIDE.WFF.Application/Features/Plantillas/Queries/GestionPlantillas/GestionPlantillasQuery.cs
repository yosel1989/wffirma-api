using COFIDE.WFF.Application.Common.Queries;

namespace COFIDE.WFF.Application.Features.Plantillas.Queries.GestionPlantillas;

public sealed record GestionPlantillasQuery(
    string? Fechainicio,
    string? Fechafin,
    List<long>? Gerencia,
    List<long>? Departamento,
    List<long>? Creador,
    List<long>? Aprobador,
    string? NombrePlantilla
) : GetPagedQuery<GestionPlantillasResponseDto>;
