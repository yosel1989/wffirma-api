using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Models;
namespace COFIDE.WFF.Application.Features.Solicitudes.Services;

public interface ISolicitudFlujoDiffService
{
    SolicitudFlujoDiff CalcularDiff(Plantilla plantilla, PlantillaEditModel request);
}
