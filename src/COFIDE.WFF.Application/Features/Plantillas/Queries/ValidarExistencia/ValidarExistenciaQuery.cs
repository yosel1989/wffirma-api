using COFIDE.WFF.Application.Common.Queries;
namespace COFIDE.WFF.Application.Features.Plantillas.Queries.ValidarExistencia;

public sealed record ValidarExistenciaQuery(string NombrePlantilla, bool Visibilidad) : BaseQuery<ValidarExistenciaResponseDto?>;