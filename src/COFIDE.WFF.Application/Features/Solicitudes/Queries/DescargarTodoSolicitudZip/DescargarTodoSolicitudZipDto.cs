namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.DescargarTodoSolicitudZip;
public class DescargarTodoSolicitudZipDto
{
    public byte[] Archivo { get; set; } = null!;
    public string NombreArchivo { get; set; } = null!;
    public string ContentType { get; set; } = null!;
}