using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Errors;
using COFIDE.WFF.Domain.Exceptions;
namespace COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Vo;

public sealed record NombrePlantilla
{
    public const int MaxLongitud = 100;
    public string Valor { get; }

    private NombrePlantilla(string valor)
    {
        Valor = valor;
    }

    public static NombrePlantilla Crear(string? valor)
    {
        var limpio = valor?.Trim() ?? string.Empty;

        if (limpio.Length > MaxLongitud)
            throw new DomainException(PlantillaErrors.LongitudNombrePlantilla.Format(MaxLongitud));

        return new NombrePlantilla(limpio);
    }

    public override string ToString() => Valor;
}
