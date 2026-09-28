using COFIDE.WFF.Application.Features.Solicitudes.DTOs;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.DescargarReporteSolicitudExcel;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerSolicitudPanelControl;
namespace COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;

public interface ISolicitudQueryRepository
{

    Task<List<SolicitudPaginadoDto>> ObtenerListadoSolicitud(ReporteSolicitudExcelRequest request, CancellationToken cancellationToken, bool tracking = false);

    Task<int> ContarListadoSolicitudFirma(
    long? codigoUsuario,
    string? fechainicio,
    string? fechafin,
    List<long>? gerencia,
    List<long>? departamento,
    List<long>? solicitante,
    List<long>? tiposolicitud,
    List<long>? subtiposolicitud,
    string? codigosolicitud,
    string? asunto,
    string? referencia,
    bool? urgente,
    CancellationToken cancellationToken,
    bool tracking = false);

    Task<SolicitudPanelControlDto?> ObtenerSolicitudPanelControl(long? codigoUsuario, CancellationToken cancellationToken, bool tracking = false);

    Task<SolicitudDto?> ObtenerSolicitudPorId(long id, long? idUsuario, bool esReuso, CancellationToken cancellationToken, bool tracking = false);
}

