namespace COFIDE.WFF.Architecture.Tests.Support;

/// <summary>
/// Deuda arquitectónica conocida y aceptada temporalmente.
///
/// Está VACÍA a propósito: la primera ejecución muestra el estado real del código
/// (cada prueba que falla indica el hallazgo del informe al que corresponde).
///
/// Para usar las pruebas en el pipeline sin bloquearlo:
///   1. Ejecute las pruebas.
///   2. Copie aquí el bloque que imprime cada prueba fallida ("Si la deuda es conocida...").
///   3. A partir de ese momento las pruebas fallan solo si aparece deuda NUEVA
///      o si algo registrado aquí se corrigió (y debe quitarse de la lista).
/// </summary>
public static class LineaBase
{
    private static readonly Dictionary<string, HashSet<string>> _violacionesConocidas = new(StringComparer.Ordinal)
    {
        // Ejemplo:
        // ["D6 - Entidades sin setters públicos"] = new(StringComparer.Ordinal)
        // {
        //     "COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Solicitud.TipoSolicitud",
        // },
    };

    public static IReadOnlySet<string> Para(string regla) =>
        _violacionesConocidas.TryGetValue(regla, out var conocidas)
            ? conocidas
            : new HashSet<string>(StringComparer.Ordinal);
}
