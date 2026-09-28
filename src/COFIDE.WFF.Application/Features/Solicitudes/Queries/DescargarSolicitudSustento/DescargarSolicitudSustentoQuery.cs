using COFIDE.WFF.Application.Common.Queries;
namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.DescargarSolicitudSustento;
public sealed record DescargarSolicitudSustentoQuery(long IdSolicitudSustento) : BaseQuery<DescargarSolicitudSustentoDto>;