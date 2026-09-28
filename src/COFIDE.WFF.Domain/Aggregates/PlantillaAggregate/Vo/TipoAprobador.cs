using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Errors;
using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Domain.Exceptions;
namespace COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Vo;

public sealed class TipoAprobador
{
    public string Codigo { get; }
    public string Nombre { get; }

    public bool EsFirmante => Codigo == Firmante.Codigo;
    public bool EsVisador => Codigo == Visador.Codigo;

    private TipoAprobador(string codigo, string nombre)
    {
        Codigo = codigo;
        Nombre = nombre;
    }

    public static readonly TipoAprobador Firmante =
        new("TI_APROBADOR_FIRMANTE", "FIRMANTE");

    public static readonly TipoAprobador Visador =
        new("TI_APROBADOR_VISADOR", "VISADOR");

    public static TipoAprobador FromCodigo(string codigo)
    {
        return codigo switch
        {
            "TI_APROBADOR_FIRMANTE" => Firmante,
            "TI_APROBADOR_VISADOR" => Visador,
            _ => throw new DomainException(PlantillaErrors.TipoAprobadorInvalido)
        };
    }

    public static TipoAprobador ObtenerTipoAprobador(long id, Dictionary<long, ParametroDetalle> dict)
    {
        if (!dict.TryGetValue(id, out var parametro))
            throw new DomainException(PlantillaErrors.TipoAprobadorInvalido);

        return TipoAprobador.FromCodigo(parametro.CodigoDetalle);
    }
}
