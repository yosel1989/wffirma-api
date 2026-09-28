using COFIDE.WFF.Application.Common.Queries;

namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.DetectarConflictosCodigosReferencia;

public sealed record DetectarCodigoQuery(
    long? IdSolicitud,
    IReadOnlyCollection<string> CodigosReferencia
) : BaseQuery<DetectarConflictosResponse>;
