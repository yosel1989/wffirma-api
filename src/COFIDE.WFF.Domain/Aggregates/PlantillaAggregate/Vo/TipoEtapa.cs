using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Errors;
using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Domain.Exceptions;
namespace COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Vo;

public sealed class TipoEtapa
{
    public string Codigo { get; }
    public string Nombre { get; }

    private TipoEtapa(string codigo, string nombre)
    {
        Codigo = codigo;
        Nombre = nombre;
    }

    public static readonly TipoEtapa Secuencial =
        new("TI_ETAPA_SECUENCIAL", "SECUENCIAL");

    public static readonly TipoEtapa Paralelo =
        new("TI_ETAPA_PARALELO", "PARALELO");

    public static TipoEtapa FromCodigo(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new DomainException(PlantillaErrors.TipoEtapaInvalido);

        return codigo switch
        {
            var c when c == Secuencial.Codigo => Secuencial,
            var c when c == Paralelo.Codigo => Paralelo,
            _ => throw new DomainException(PlantillaErrors.TipoEtapaInvalido)
        };
    }

    public bool PermiteMultiplesFirmantes()
        => this == Paralelo;

    public void ValidarCantidadFirmantes(int cantidad)
    {
        if (this == Secuencial && cantidad != 1)
            throw new DomainException(PlantillaErrors.LimiteFirmantesSecuencial);

        if (this == Paralelo && cantidad < 2)
            throw new DomainException(PlantillaErrors.LimiteFirmantesParalelo);
    }

    public static TipoEtapa ObtenerTipoEtapa(long id, Dictionary<long, ParametroDetalle> dict)
    {
        if (!dict.TryGetValue(id, out var parametro))
            throw new DomainException(PlantillaErrors.TipoEtapaInvalido);

        return TipoEtapa.FromCodigo(parametro.CodigoDetalle);
    }
}
