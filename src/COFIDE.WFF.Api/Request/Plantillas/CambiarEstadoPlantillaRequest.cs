namespace COFIDE.WFF.Api.Request.Plantillas;

/// <summary>Estado de activación solicitado para una plantilla.</summary>
public sealed record CambiarEstadoPlantillaRequest
{
    public required bool Activo { get; init; }
}
