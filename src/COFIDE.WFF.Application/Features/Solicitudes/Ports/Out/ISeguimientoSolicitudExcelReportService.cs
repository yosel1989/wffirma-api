using COFIDE.WFF.Application.Features.Solicitudes.Queries.SeguimientoSolicitudes;

namespace COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;

public interface ISeguimientoSolicitudExcelReportService
{
    byte[] GenerarReporteSeguimiento(List<SeguimientoSolicitudesDto> solicitudes);
}
