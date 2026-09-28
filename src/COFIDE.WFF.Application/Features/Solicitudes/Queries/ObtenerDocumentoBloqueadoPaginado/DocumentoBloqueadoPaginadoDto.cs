namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerDocumentoBloqueadoPaginado;

public class DocumentoBloqueadoPaginadoDto
{
    public string? IdArchivo { get; set; }
    public string? NombreArchivo { get; set; }
    public string? CodigoSolicitud { get; set; }
    public string? FirmanteAnterior { get; set; }
    public string? FirmanteActual { get; set; }
    public DateTime? FechaBloqueo { get; set; }
    public string? Estado { get; set; }
}
