using COFIDE.WFF.Domain.Common.ValueObjects;
using COFIDE.WFF.Domain.Exceptions;
namespace COFIDE.WFF.Domain.ValueObjects;

public sealed class InternoExterno : ValueObject
{
    public static readonly InternoExterno Externo = new InternoExterno('E', "Externo");
    public static readonly InternoExterno Interno = new InternoExterno('I', "Interno");

    public char Codigo { get; }
    public string Nombre { get; }

    private InternoExterno(char codigo, string nombre)
    {
        Codigo = codigo;
        Nombre = nombre;
    }

    public static InternoExterno? FromCodigo(char codigo) =>
        codigo switch
        {
            'E' => Externo,
            'I' => Interno,
            _ => null
        };

    public static InternoExterno? FromString(string? codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            return null;

        return FromCodigo(codigo.Trim()[0]);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Codigo;
    }

    public static bool EsValido(char codigo)
    {
        return codigo == 'E' || codigo == 'I';
    }

    public override string ToString() => Nombre;
}