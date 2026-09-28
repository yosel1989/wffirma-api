using COFIDE.WFF.Application.Common.Queries;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.DescargarReporteSolicitudExcel;
namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.DescargarSolicitudExcel;

public sealed record DescargarReporteSolicitudExcelQuery(
    string? fechainicio,
    string? fechafin,
    List<long>? gerencia,
    List<long>? departamento,
    List<long>? solicitante,
    List<long>? tiposolicitud,
    List<long>? subtiposolicitud,
    List<long>? tipoOperacion,
    List<long>? aprobadorActual,
    List<long>? estado,
    string? codigosolicitud,
    string? asunto,
    string? referencia,
    bool? urgente,
    bool? usuario,
    bool? usuariodepartamento
) : BaseQuery<ReporteSolicitudExcelDto>;