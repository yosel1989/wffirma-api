namespace COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;

/// <summary>
/// Representa el estado lógico de una plantilla.
/// </summary>
public enum EstadoPlantilla
{
    /// <summary>
    /// La plantilla está activa.
    /// </summary>
    Activa,

    /// <summary>
    /// La plantilla está inactiva.
    /// </summary>
    Inactiva,

    /// <summary>
    /// La plantilla fue eliminada lógicamente.
    /// </summary>
    Eliminada
}
