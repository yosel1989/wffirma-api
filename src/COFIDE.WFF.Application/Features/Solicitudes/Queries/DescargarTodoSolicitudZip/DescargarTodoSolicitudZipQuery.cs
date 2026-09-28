using COFIDE.WFF.Application.Common.Queries;
namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.DescargarTodoSolicitudZip;
public sealed record DescargarTodoSolicitudZipQuery(long Id) : BaseQuery<DescargarTodoSolicitudZipDto>;