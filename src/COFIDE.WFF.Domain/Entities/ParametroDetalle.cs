using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Vo;
using COFIDE.WFF.Domain.Common.Entities;

namespace COFIDE.WFF.Domain.Entities;

public class ParametroDetalle : AuditableEntity<long>
{
    public long IdParametroCabecera { get; private set; }
    public string CodigoDetalle { get; private set; } = default!;
    public string? DescripcionDetalle { get; private set; }
    public string ValorParametro { get; private set; } = default!;
    public ParametroCabecera Cabecera { get; private set; } = null!;

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
