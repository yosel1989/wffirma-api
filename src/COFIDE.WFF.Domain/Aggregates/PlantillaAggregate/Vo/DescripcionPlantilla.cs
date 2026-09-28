using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Errors;
using COFIDE.WFF.Domain.Exceptions;
namespace COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Vo;

public sealed record DescripcionPlantilla
{
    public const int MaxLongitud = 500;

    public string Valor { get; }

    private DescripcionPlantilla(string valor)
    {
        Valor = valor;
    }

    public static DescripcionPlantilla? Crear(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return null;

        var limpio = valor.Trim();

        if (limpio.Length > MaxLongitud)
            throw new DomainException(PlantillaErrors.LongitudDescripcionPlantilla.Format(MaxLongitud));

        return new DescripcionPlantilla(limpio);
    }

    public override string ToString() => Valor;
}