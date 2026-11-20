using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;

namespace COFIDE.WFF.Application.Features.Plantillas.Services;

/// <summary>
/// Autoriza los cambios de estado lógico de una plantilla.
/// </summary>
public interface IPlantillaCambioEstadoAuthorizationService
{
    /// <summary>
    /// Valida que el usuario actual pueda cambiar el estado de la plantilla.
    /// </summary>
    Task AutorizarAsync(Plantilla plantilla, CancellationToken cancellationToken);

    /// <summary>
    /// Indica si el usuario actual puede cambiar el estado de la plantilla.
    /// </summary>
    Task<bool> PuedeAutorizarAsync(Plantilla plantilla, CancellationToken cancellationToken);
}
