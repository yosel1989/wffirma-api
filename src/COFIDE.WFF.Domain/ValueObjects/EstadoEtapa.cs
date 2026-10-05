namespace COFIDE.WFF.Domain.ValueObjects;

/// <summary>
/// Estado de una etapa del flujo de aprobación de una solicitud.
/// Bloqueado = la etapa ya tiene aprobaciones o firmas y no puede modificarse.
/// </summary>
public sealed record EstadoEtapa(long OrdenEtapa, bool Bloqueado);
