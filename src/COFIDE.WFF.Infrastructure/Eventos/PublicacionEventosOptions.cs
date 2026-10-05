namespace COFIDE.WFF.Infrastructure.Eventos;

public sealed class PublicacionEventosOptions
{
    public const string SectionName = "PublicacionEventos";

    /// <summary>Reintentos en segundo plano cuando la publicación inmediata falla.</summary>
    public int MaximoReintentos { get; set; } = 3;

    /// <summary>Espera antes de cada reintento (se multiplica por el número de intento).</summary>
    public int SegundosEntreReintentos { get; set; } = 10;

    /// <summary>Eventos que pueden quedar en espera de reintento al mismo tiempo.</summary>
    public int CapacidadCola { get; set; } = 1000;
}
