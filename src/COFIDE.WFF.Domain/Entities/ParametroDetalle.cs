using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Vo;
using COFIDE.WFF.Domain.Common.Entities;

namespace COFIDE.WFF.Domain.Entities;

public class ParametroDetalle : AuditableEntity<long>
{
    public long IdParametroCabecera { get; set; }
    public string CodigoDetalle { get; set; } = default!;
    public string? DescripcionDetalle { get; set; }
    public string ValorParametro { get; set; } = default!;
    public ParametroCabecera Cabecera { get; set; } = null!;

    private ParametroDetalle() { } // EF   

    public ParametroDetalle(
        long id,
        string codigoDetalle,
        string valorParametro)
    {
        Id = id;
        CodigoDetalle = codigoDetalle;
        ValorParametro = valorParametro;
    }
}
