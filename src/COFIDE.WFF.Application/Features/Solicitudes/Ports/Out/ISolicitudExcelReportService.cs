using COFIDE.WFF.Application.Features.Solicitudes.DTOs;

namespace COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;

public interface ISolicitudExcelReportService
{
    byte[] GenerarReporteSolicitudes(List<SolicitudPaginadoDto> solicitudes);
}
