using COFIDE.WFF.Application.Common.Queries;
namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.GenerarPdfSolicitud;

public sealed record GenerarPdfSolicitudQuery(long IdSolicitud) : BaseQuery<GenerarPdfSolicitudDto?>;
