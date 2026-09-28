using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Errors;
using COFIDE.WFF.Domain.Common.ValueObjects;
using COFIDE.WFF.Domain.Exceptions;
namespace COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Vo;
public sealed class FechaLimite : ValueObject
{
    public DateTime Valor { get; }

    private FechaLimite() { } // EF

    private FechaLimite(DateTime valor)
    {
        Valor = valor;
    }

    public static FechaLimite Crear(DateTime valor)
    {
        if (valor < DateTime.Now)
            throw new DomainException(SolicitudErrors.FechaLimiteFuturo);

        return new FechaLimite(valor);
    }

    public bool EstaVencida()
        => Valor < DateTime.Now;

    public bool EstaPorVencer(TimeSpan margen)
        => Valor - DateTime.Now <= margen;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Valor;
    }

    public override string ToString()
        => Valor.ToString("yyyy-MM-dd HH:mm");
}