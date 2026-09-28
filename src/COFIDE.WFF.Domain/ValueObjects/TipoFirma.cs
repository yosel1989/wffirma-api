using COFIDE.WFF.Domain.Common.ValueObjects;
using COFIDE.WFF.Domain.Exceptions;
namespace COFIDE.WFF.Domain.ValueObjects;

public sealed class TipoFirma : ValueObject
{
    public static readonly TipoFirma FirmaEsquinaSuperiorDerecha = new(0, "FirmaEsquinaSuperiorDerecha");
    public static readonly TipoFirma FirmaEsquinaSuperiorIzquierda = new(1, "FirmaEsquinaSuperiorIzquierda");
    public static readonly TipoFirma FirmaInvisible = new(2, "FirmaInvisible");
    public static readonly TipoFirma FirmaConTag = new(3, "FirmaConTag");
    public static readonly TipoFirma FirmaConParametros = new(4, "FirmaConParametros");

    private static readonly Dictionary<long, TipoFirma> _estados =
    new()
        {
            {0, FirmaEsquinaSuperiorDerecha},
            {1, FirmaEsquinaSuperiorIzquierda},
            {2, FirmaInvisible},
            {3, FirmaConTag},
            {4, FirmaConParametros}
        };

    public int Codigo { get; }
    public string Nombre { get; }

    private TipoFirma(int codigo, string nombre)
    {
        Codigo = codigo;
        Nombre = nombre;
    }

    public static TipoFirma FromCodigo(int codigo)
    {
        if (!_estados.TryGetValue(codigo, out var estado))
            throw new DomainException(new Common.Errors.DomainError("", $"Tipo de Firma inválido: {codigo}"));

        return estado;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Codigo;
    }

    public override string ToString() => Nombre;
}