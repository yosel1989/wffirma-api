using COFIDE.WFF.Application.Common.Queries;
namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerSolicitud;

public sealed record ObtenerSolicitudQuery(long IdSolicitud, bool EsReuso) : BaseQuery<SolicitudDto?>;