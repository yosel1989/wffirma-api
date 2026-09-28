namespace COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Vo;

public sealed record NombreEtapa
{
    public const int MaxLongitud = 15;
    public string Valor { get; private set; } = null!;
    private NombreEtapa() { }
    private NombreEtapa(string valor)
    {
        Valor = valor;
    }

    public static NombreEtapa Crear(string? valor)
    {
        var limpio = valor!.Trim();

        return new NombreEtapa(limpio);
    }

    public void Actualizar(string valor)
    {
        Valor = valor.Trim();
    }

    public override string ToString() => Valor;
}
