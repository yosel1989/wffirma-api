using COFIDE.WFF.Application.Common.Queries;
namespace COFIDE.WFF.Application.Features.Plantillas.Queries.PermisoEditarPlantilla;

public sealed record PermisoEditarPlantillaQuery(long Id) : BaseQuery<bool>;
