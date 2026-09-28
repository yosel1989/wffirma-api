using COFIDE.WFF.Application.Common.Commands;
using COFIDE.WFF.Application.Features.Plantillas.Models;
namespace COFIDE.WFF.Application.Features.Plantillas.Commands.RegistrarPlantilla;
public sealed record RegistrarPlantillaCommand
(
    string? NombrePlantilla,
    string? Descripcion,
    bool? Visibilidad,
    bool? EsReutilizable,
    List<PlantillaEtapaRequest>? Etapas
) : CreateCommand<long>;