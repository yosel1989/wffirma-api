using COFIDE.WFF.Application.Features.Solicitudes.Queries.SeguimientoSolicitudes;

namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.DescargarReporteSolicitudExcel;

public sealed class ReporteSolicitudExcelDto
{
    public byte[] Contenido { get; set; } = Array.Empty<byte>();

    public string ContentType { get; set; } =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public string NombreArchivo { get; set; } = default!;
}

public class ReportSeguimientoSolicitudesDto
{
    public long IdSolicitud { get; set; }
    public string? CodigoSolicitud { get; set; }
    public string? Solicitante { get; set; }
    public string? TipoSolicitud { get; set; }
    public string? SubTipoSolicitud { get; set; }
    public string? TipoOperacion { get; set; }
    public string? Urgente { get; set; }
    public string? Estado { get; set; }
    public DateTime? FechaCreacion { get; set; }
    public List<FirmantesDto>? Firmantes { get; set; }

    public string Aprobadores => string.Join(", ",
    (Firmantes ?? []).Select(f => $"{f.Nombre} ({Sigla(f.Estado)})"));

    private static string Sigla(string? estado) => estado switch
    {
        "APROBADO" => "F",
        "RECHAZADO" => "R",
        "PENDIENTE" => "P",
        _ => ""
    };
}