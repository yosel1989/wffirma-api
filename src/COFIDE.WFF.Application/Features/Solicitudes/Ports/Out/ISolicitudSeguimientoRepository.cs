

using COFIDE.WFF.Application.Common.Pageds;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.DescargarReporteSeguimientoExcel;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.SeguimientoSolicitudes;

namespace COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;

public interface ISolicitudSeguimientoRepository
{
    Task<PagedResult<SeguimientoSolicitudesDto, SeguimientoSolicitudesResumenDto>> ObtenerPaginadoSeguimiento(
        PagedQueryParams paginationParams,
        long? codigoUsuario,
        long? areaUsuario,
        string? roLUsuario,
        long? departamentoUsuario,
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
        bool? rechazada,
        bool? incluirresumen,
        CancellationToken cancellationToken);

    List<SeguimientoSolicitudesDto> ObtenerReporteSeguimiento(
       ReporteSeguimientoExcelRequest request, CancellationToken cancellationToken);
}
