namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.GenerarPdfSolicitud;

public class GenerarPdfSolicitudDto
{
    public byte[] Archivo { get; set; } = null!;
    public string NombreArchivo { get; set; } = null!;
    public string ContentType { get; set; } = "application/pdf";
}
