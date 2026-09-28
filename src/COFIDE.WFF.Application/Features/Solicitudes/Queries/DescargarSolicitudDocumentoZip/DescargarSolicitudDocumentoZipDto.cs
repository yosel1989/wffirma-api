namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.DescargarSolicitudDocumentoZip;

public class DescargarSolicitudDocumentoZipDto
{
    public byte[] Archivo { get; set; } = null!;
    public string NombreArchivo { get; set; } = null!;
    public string ContentType { get; set; } = null!;
}