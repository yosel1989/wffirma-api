using COFIDE.WFF.Application.Common.Queries;

namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerPuntosRestauracion;

public sealed record ObtenerPuntosRestauracionQuery(long IdSolicitud)
    : BaseQuery<IReadOnlyList<PuntoRestauracionDto>>;
