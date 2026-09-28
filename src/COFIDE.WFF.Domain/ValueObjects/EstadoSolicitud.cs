using COFIDE.WFF.Domain.Common.ValueObjects;
using COFIDE.WFF.Domain.Exceptions;
namespace COFIDE.WFF.Domain.ValueObjects;

public sealed class EstadoSolicitud : ValueObject
{
    public static readonly EstadoSolicitud Borrador = new(1, "Borrador");
    public static readonly EstadoSolicitud EnCurso = new(2, "En Curso");
    public static readonly EstadoSolicitud Aprobado = new(3, "Aprobado");
    public static readonly EstadoSolicitud Rechazado = new(4, "Rechazado");
    public static readonly EstadoSolicitud Anulado = new(5, "Anulado");

    private static readonly Dictionary<long, EstadoSolicitud> _estados =
    new()
        {
            {1, Borrador},
            {2, EnCurso},
            {3, Aprobado},
            {4, Rechazado},
            {5, Anulado}
        };

    public long Codigo { get; }
    public string Nombre { get; }

    private EstadoSolicitud(long codigo, string nombre)
    {
        Codigo = codigo;
        Nombre = nombre;
    }

    public static EstadoSolicitud FromCodigo(long codigo)
    {
        if (!_estados.TryGetValue(codigo, out var estado))
            throw new DomainException(new Common.Errors.DomainError("", $"Estado Solicitud inválido: {codigo}"));

        return estado;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Codigo;
    }

    public override string ToString() => Nombre;
}