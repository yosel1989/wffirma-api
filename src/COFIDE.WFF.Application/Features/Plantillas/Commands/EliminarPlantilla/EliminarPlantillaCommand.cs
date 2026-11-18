using COFIDE.WFF.Application.Common.Commands;

namespace COFIDE.WFF.Application.Features.Plantillas.Commands.EliminarPlantilla;

/// <summary>
/// Solicita la eliminación lógica de una plantilla.
/// </summary>
public sealed record EliminarPlantillaCommand(long Id)
    : UpdateCommand<bool, long>(Id);
