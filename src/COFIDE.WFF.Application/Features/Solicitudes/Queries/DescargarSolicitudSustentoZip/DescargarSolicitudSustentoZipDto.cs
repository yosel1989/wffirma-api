namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.DescargarSolicitudSustentoZip;
public class DescargarSolicitudSustentoZipDto
{
   public byte[] Archivo { get; set; } = null!;
    public string NombreArchivo { get; set; } = null!;
    public string ContentType { get; set; } = null!;
}

