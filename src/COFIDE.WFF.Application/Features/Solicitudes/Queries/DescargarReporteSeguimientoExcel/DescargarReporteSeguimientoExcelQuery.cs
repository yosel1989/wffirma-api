using COFIDE.WFF.Application.Common.Queries;

namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.DescargarReporteSeguimientoExcel;

public sealed record DescargarReporteSeguimientoExcelQuery(
    string? fechainicio,
    string? fechafin,
    List<long>? gerencia,
    List<long>? departamento,
    List<long>? solicitante,
    List<long>? tiposolicitud,
    List<long>? subtiposolicitud,
    List<long>? tipoOperacion,
    string? asunto,
    string? referencia,
    List<long>? estado,
    List<long>? firmante,
    string? codigosolicitud,
    bool? encurso,
    bool? aprobada,
    bool? rechazada
    ) : GetPagedQuery<ReporteSeguimientoExcelDto>;