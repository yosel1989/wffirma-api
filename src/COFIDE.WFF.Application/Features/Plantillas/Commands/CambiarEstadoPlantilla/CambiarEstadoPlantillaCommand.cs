using COFIDE.WFF.Application.Common.Commands;

namespace COFIDE.WFF.Application.Features.Plantillas.Commands.CambiarEstadoPlantilla;

public sealed record CambiarEstadoPlantillaCommand(long Id, bool Activo)
    : UpdateCommand<bool, long>(Id);
