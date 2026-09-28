using COFIDE.WFF.Application.Common.Queries;
namespace COFIDE.WFF.Application.Features.Plantillas.Queries.ObtenerPorId;

public sealed record ObtenerPlantillaPorIdQuery(long IdPlantilla) : BaseQuery<PlantillaDto?>;