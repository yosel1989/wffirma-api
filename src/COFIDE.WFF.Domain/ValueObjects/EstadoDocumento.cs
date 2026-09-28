using COFIDE.WFF.Domain.Common.ValueObjects;
using COFIDE.WFF.Domain.Exceptions;
namespace COFIDE.WFF.Domain.ValueObjects;

public sealed class EstadoDocumento : ValueObject
{
    public static readonly EstadoDocumento Pendiente = new(1, "Pendiente");
    public static readonly EstadoDocumento Bloqueado = new(2, "Bloqueado");
    public static readonly EstadoDocumento Firmado = new(3, "Firmado");
    public static readonly EstadoDocumento Fallido = new(4, "Fallido");

    private static readonly Dictionary<long, EstadoDocumento> _estados =
    new()
    {
        {1, Pendiente},
        {2, Bloqueado},
        {3, Firmado},
        {4, Fallido}
    };

    public long Codigo { get; }
    public string Nombre { get; }

    private EstadoDocumento(long codigo, string nombre)
    {
        Codigo = codigo;
        Nombre = nombre;
    }

    public static EstadoDocumento FromCodigo(long codigo)
    {
        if (!_estados.TryGetValue(codigo, out var estado))
            throw new DomainException(new Common.Errors.DomainError("", $"Estado Documento inválido: {codigo}"));

        return estado;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Codigo;
    }

    public override string ToString() => Nombre;
}
