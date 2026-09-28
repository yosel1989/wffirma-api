using COFIDE.WFF.Application.Common.Queries;
namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.DescargarSolicitudDocumento;

public sealed record DescargarSolicitudDocumentoQuery(long IdSolicitudDocumento) : BaseQuery<DescargarSolicitudDocumentoDto>;
