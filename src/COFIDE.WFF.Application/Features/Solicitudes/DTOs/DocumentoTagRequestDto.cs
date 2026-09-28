namespace COFIDE.WFF.Application.Features.Solicitudes.DTOs;

public sealed record DocumentoTagRequestDto(
    long? IdSolicitudDocumento,
    string NombreDocumento,
    List<Coordenada> Coordenadas
);


public class Coordenada
{
    public long? Id { get; set; }
    public int NumeroPagina { get; set; }
    public decimal X { get; set; }
    public decimal Y { get; set; }
}