namespace COFIDE.WFF.Domain.Common.Tiempo;

/// <summary>
/// Fecha y hora "actual" para todo el núcleo (Domain y Application).
/// Reemplaza a DateTime.Now para que:
///   - la hora no dependa de la zona horaria del servidor (se configura al iniciar: hora de Lima),
///   - las reglas que dependen del tiempo se puedan probar fijando la fecha.
/// </summary>
public static class HoraSistema
{
    private static Func<DateTime> _proveedor = () => TimeProvider.System.GetLocalNow().DateTime;
    private static readonly AsyncLocal<DateTime?> _fijada = new();

    /// <summary>Fecha y hora actual en la zona horaria configurada.</summary>
    public static DateTime Ahora => _fijada.Value ?? _proveedor();

    /// <summary>Fecha actual (sin hora) en la zona horaria configurada.</summary>
    public static DateTime Hoy => Ahora.Date;

    /// <summary>Se llama una vez al iniciar la aplicación.</summary>
    public static void Configurar(TimeProvider proveedor, TimeZoneInfo zonaHoraria)
    {
        ArgumentNullException.ThrowIfNull(proveedor);
        ArgumentNullException.ThrowIfNull(zonaHoraria);
        _proveedor = () => TimeZoneInfo.ConvertTime(proveedor.GetUtcNow(), zonaHoraria).DateTime;
    }

    /// <summary>
    /// Fija la hora para el flujo actual (pruebas). Al liberar el objeto devuelto se restaura.
    /// <code>using var _ = HoraSistema.Fijar(new DateTime(2026, 12, 31, 23, 59, 0));</code>
    /// </summary>
    public static IDisposable Fijar(DateTime fecha)
    {
        var anterior = _fijada.Value;
        _fijada.Value = fecha;
        return new Restauracion(() => _fijada.Value = anterior);
    }

    private sealed class Restauracion(Action restaurar) : IDisposable
    {
        private Action? _restaurar = restaurar;

        public void Dispose()
        {
            _restaurar?.Invoke();
            _restaurar = null;
        }
    }
}
