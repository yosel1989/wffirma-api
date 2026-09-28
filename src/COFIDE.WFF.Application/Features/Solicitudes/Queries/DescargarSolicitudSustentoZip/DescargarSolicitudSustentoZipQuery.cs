using COFIDE.WFF.Application.Common.Queries;
namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.DescargarSolicitudSustentoZip;

public sealed record DescargarSolicitudSustentoZipQuery(long Id,List<long>? Sustentos) : BaseQuery<DescargarSolicitudSustentoZipDto>;

