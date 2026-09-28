namespace COFIDE.WFF.Domain.ValueObjects;
public class ResultadoValidacionFirma
{
    public bool EsValido { get; private set; }
    public string? Mensaje { get; private set; }

    public static ResultadoValidacionFirma Ok() =>
        new ResultadoValidacionFirma { EsValido = true };

    public static ResultadoValidacionFirma Fail(string mensaje) =>
        new ResultadoValidacionFirma { EsValido = false, Mensaje = mensaje };
}
