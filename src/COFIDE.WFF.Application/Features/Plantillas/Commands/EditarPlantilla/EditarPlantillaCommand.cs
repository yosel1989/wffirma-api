using COFIDE.WFF.Application.Common.Commands;
using COFIDE.WFF.Application.Features.Plantillas.Models;
namespace COFIDE.WFF.Application.Features.Plantillas.Commands.EditarPlantilla;

public sealed record EditarPlantillaCommand
(
    long Id,
    string? NombrePlantilla,
    string? Descripcion,
    List<PlantillaEtapaRequest>? Etapas
) : UpdateCommand<long, long>(Id);