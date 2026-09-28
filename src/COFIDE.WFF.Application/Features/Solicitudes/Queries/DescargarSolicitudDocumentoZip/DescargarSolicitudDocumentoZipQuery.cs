using COFIDE.WFF.Application.Common.Queries;
namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.DescargarSolicitudDocumentoZip;

public sealed record DescargarSolicitudDocumentoZipQuery(long Id, List<long>? Documentos) : BaseQuery<DescargarSolicitudDocumentoZipDto>;