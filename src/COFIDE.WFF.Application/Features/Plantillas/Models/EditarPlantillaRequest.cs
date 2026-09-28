namespace COFIDE.WFF.Application.Features.Plantillas.Models;

public sealed record EditarPlantillaRequest
(
    string? NombrePlantilla,
    string? Descripcion,
    List<PlantillaEtapaRequest>? Etapas
);