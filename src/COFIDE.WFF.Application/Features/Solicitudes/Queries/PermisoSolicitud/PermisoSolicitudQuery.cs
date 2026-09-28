using COFIDE.WFF.Application.Common.Queries;
namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.PermisoSolicitud;
public sealed record PermisoSolicitudQuery(long Id, bool Revisar) : BaseQuery<bool>;