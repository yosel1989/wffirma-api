using COFIDE.WFF.Application.Common.Pageds;
using COFIDE.WFF.Application.Common.Queries;
namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerDocumentoBloqueadoPaginado;
public sealed record ObtenerDocumentoBloqueadoPaginadoQuery(
    string? FechaInicio,
    string? FechaFin,
    string? NombreArchivo,
    string? CodigoSolicitud,
    List<long>? FirmanteActual
) : GetPagedQuery<PagedResultDto<DocumentoBloqueadoPaginadoDto>>;