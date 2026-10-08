using COFIDE.WFF.Application.Features.Plantillas.Models;

namespace COFIDE.WFF.Api.Request.Plantillas
{
    /// <summary>Datos para editar una plantilla de flujo.</summary>
    public sealed class ActualizarPlantillaRequest
    {
        public string? NombrePlantilla { get; init; }
        public string? Descripcion { get; init; }
        public List<PlantillaEtapaRequest>? Etapas { get; init; }
    }
}
