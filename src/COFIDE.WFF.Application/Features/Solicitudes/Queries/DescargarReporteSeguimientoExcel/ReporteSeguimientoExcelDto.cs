namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.DescargarReporteSeguimientoExcel;

public sealed class ReporteSeguimientoExcelDto
{
    public byte[] Contenido { get; set; } = Array.Empty<byte>();

    public string ContentType { get; set; } =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public string NombreArchivo { get; set; } = default!;
}
