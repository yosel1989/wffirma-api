using COFIDE.WFF.Domain.Common.Tiempo;
using Microsoft.Extensions.Configuration;

namespace COFIDE.WFF.Infrastructure.Configuracion.Tiempo;

/// <summary>
/// Configura la zona horaria de <see cref="HoraSistema"/> al iniciar la Api o el Worker.
/// Por defecto usa la hora de Lima, sin importar la zona horaria configurada en el servidor.
/// Se puede cambiar con "Regional:ZonaHoraria" en appsettings (identificador IANA o de Windows).
/// </summary>
public static class HoraSistemaConfiguracion
{
    private const string ZonaPorDefecto = "America/Lima";
    private const string ZonaPorDefectoWindows = "SA Pacific Standard Time";

    public static void Configurar(IConfiguration configuration)
    {
        var id = configuration["Regional:ZonaHoraria"];
        HoraSistema.Configurar(TimeProvider.System, ObtenerZona(string.IsNullOrWhiteSpace(id) ? ZonaPorDefecto : id));
    }

    private static TimeZoneInfo ObtenerZona(string id)
    {
        foreach (var candidato in new[] { id, ZonaPorDefecto, ZonaPorDefectoWindows })
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(candidato);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        // Último recurso: UTC-5 fijo (Perú no usa horario de verano).
        return TimeZoneInfo.CreateCustomTimeZone("Lima", TimeSpan.FromHours(-5), "Lima", "Lima");
    }
}
