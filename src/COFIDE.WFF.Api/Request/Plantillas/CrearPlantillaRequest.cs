using COFIDE.WFF.Application.Features.Plantillas.Models;

namespace COFIDE.WFF.Api.Request.Plantillas
{
    /// <summary>Datos para registrar una plantilla de flujo.</summary>
    public sealed class CrearPlantillaRequest
    {
        public string? NombrePlantilla { get; init; }
        public string? Descripcion { get; init; }
        public bool? Visibilidad { get; init; }
        public bool? EsReutilizable { get; init; }
        public List<PlantillaEtapaRequest>? Etapas { get; init; }
    }
}
