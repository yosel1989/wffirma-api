using COFIDE.WFF.Domain.Exceptions;

namespace COFIDE.WFF.Domain.ValueObjects;

public sealed class CodigoSolicitud
{
    public string Value { get; }

    private CodigoSolicitud(string value)
    {
        Value = value;
    }

    public static CodigoSolicitud Crear(
        int anio,
        string areaCodigo,
        string tipoCodigo,
        long correlativo)
    {
        var codigo = $"{areaCodigo}-{tipoCodigo}-{correlativo:D8}-{anio}";
        return new CodigoSolicitud(codigo);
    }

    public override string ToString() => Value;
}