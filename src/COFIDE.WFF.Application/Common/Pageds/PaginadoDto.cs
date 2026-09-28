namespace COFIDE.WFF.Application.Common.Pageds;

public class PaginadoDto
{
    public int NumeroPagina { get; init; }
    public int TamanoDePagina { get; init; }
    public int NumeroDeElementos { get; init; }
    public int PaginasTotales { get; init; }
    public int TotalElementos { get; init; }
    public bool Vacio { get; init; }
    public bool Primero { get; init; }
    public bool Ultimo { get; init; }
}
