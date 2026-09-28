namespace COFIDE.WFF.Application.Features.Maestros.Queries.ObtenerListadoParametro;

public class ListadoParametroDto
{
    public string CodigoParametro { get; set; } = default!;
    public long IdParametroDetalle { get; set; }
    public string CodigoDetalle { get; set; } = default!;
    public string ValorParametro { get; set; } = default!;
}
