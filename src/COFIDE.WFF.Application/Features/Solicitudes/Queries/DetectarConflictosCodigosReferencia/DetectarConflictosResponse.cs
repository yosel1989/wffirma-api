namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.DetectarConflictosCodigosReferencia;

public sealed record DetectarConflictosResponse
(
    int CantidadNoAñadida,
    IReadOnlyCollection<CodigoReferenciaConflictoDto> Conflictos
);
