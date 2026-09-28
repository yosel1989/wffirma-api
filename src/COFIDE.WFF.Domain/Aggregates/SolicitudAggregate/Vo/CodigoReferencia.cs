using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Errors;
using COFIDE.WFF.Domain.Exceptions;

namespace COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Vo;

public sealed record CodigoReferencia
{
    public const int MaxLongitud = 50;

    public string Valor { get; }

    private CodigoReferencia(string valor)
    {
        Valor = valor;
    }

    public static CodigoReferencia Crear(string? valor, int maxLongitud = MaxLongitud)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new DomainException(SolicitudErrors.CodReferenciaRequired);

        var limpio = valor.Trim();

        if (limpio.Length > maxLongitud)
            throw new DomainException(SolicitudErrors.CodReferenciaLength.Format(maxLongitud));

        return new CodigoReferencia(limpio);
    }

    public override string ToString() => Valor;
}
