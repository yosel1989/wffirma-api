using COFIDE.WFF.Application.Features.Solicitudes.Queries.SeguimientoSolicitudes;

namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.DescargarReporteSolicitudExcel;

public sealed class ReporteSolicitudExcelDto
{
    public byte[] Contenido { get; set; } = Array.Empty<byte>();

    public string ContentType { get; set; } =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public string NombreArchivo { get; set; } = default!;
}