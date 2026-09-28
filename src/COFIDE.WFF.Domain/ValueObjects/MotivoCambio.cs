using COFIDE.WFF.Domain.Exceptions;
namespace COFIDE.WFF.Domain.ValueObjects;

public sealed record  MotivoCambio
{
    public const int MaxLongitud = 300;

    public string Valor { get; }

    private MotivoCambio(string valor)
    {
        Valor = valor;
    }

    public static MotivoCambio? Crear(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return null;

        var limpio = valor.Trim();

        return new MotivoCambio(limpio);
    }

    public override string ToString() => Valor;
}
