using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Inputs;
namespace COFIDE.WFF.Application.Features.Plantillas.Services;

public interface IPlantillaCreacionService
{
    Task<Plantilla> CrearPlantilla(CrearPlantillaInput input, long usuarioActual, string gerenciaUsuario, CancellationToken cancellationToken);

    /// <summary>
    /// Crea una plantilla con los identificadores territoriales disponibles del usuario.
    /// </summary>
    /// <param name="input">Datos de creación de la plantilla.</param>
    /// <param name="usuarioActual">Identificador del usuario propietario.</param>
    /// <param name="gerenciaUsuario">Gerencia del usuario propietario.</param>
    /// <param name="idAreaPlantilla">Identificador del área del usuario.</param>
    /// <param name="idAreaPadrePlantilla">Identificador del área padre del usuario.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>La plantilla creada.</returns>
    Task<Plantilla> CrearPlantilla(
        CrearPlantillaInput input,
        long usuarioActual,
        string gerenciaUsuario,
        long? idAreaPlantilla,
        long? idAreaPadrePlantilla,
        CancellationToken cancellationToken);
}
