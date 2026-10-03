using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Models;
namespace COFIDE.WFF.Domain.DomainServices;

public interface IFlujoDiffDomainService
{
    SolicitudFlujoDiff CalcularDiff(Plantilla plantilla, PlantillaEditModel request);
}
