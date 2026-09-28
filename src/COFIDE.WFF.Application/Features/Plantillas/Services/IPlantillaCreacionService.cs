using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Inputs;
namespace COFIDE.WFF.Application.Features.Plantillas.Services;

public interface IPlantillaCreacionService
{
    Task<Plantilla> CrearPlantilla(CrearPlantillaInput input, long usuarioActual, string gerenciaUsuario, CancellationToken cancellationToken);
}
