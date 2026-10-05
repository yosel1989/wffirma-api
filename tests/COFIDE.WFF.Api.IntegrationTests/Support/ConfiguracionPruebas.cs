using Microsoft.Extensions.Configuration;

namespace COFIDE.WFF.Api.IntegrationTests.Support;

/// <summary>
/// Configuración de las pruebas de integración. Orden de prioridad (el último gana):
///   1. appsettings.IntegrationTests.json          (valores no sensibles, versionado)
///   2. Resources/Configuracion/conexiones.json    (conexiones y credenciales de QA; en .gitignore)
///   3. variables de entorno con prefijo WFF_IT_   (pipeline)
/// Solución temporal hasta adoptar el gestor de secretos: al migrar, solo cambia este archivo.
/// </summary>
public static class ConfiguracionPruebas
{
    public const string ArchivoConexiones = "Resources/Configuracion/conexiones.json";
    public const string PrefijoVariablesEntorno = "WFF_IT_";

    private static readonly Lazy<IConfiguration> _configuracion = new(Construir);

    public static IConfiguration Configuracion => _configuracion.Value;

    public static string? CadenaConexion =>
        Configuracion.GetConnectionString("OracleConnection") is { Length: > 0 } valor ? valor : null;

    public static bool TieneBaseDeDatos => CadenaConexion is not null;

    public static bool ApisExternasHabilitadas =>
        Configuracion.GetValue<bool>("IntegrationTests:ApisExternasHabilitadas");

    public static UsuarioPrueba Usuario =>
        Configuracion.GetSection("IntegrationTests:Usuario").Get<UsuarioPrueba>() ?? new UsuarioPrueba();

    private static IConfiguration Construir() =>
        new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.IntegrationTests.json", optional: false)
            .AddJsonFile(ArchivoConexiones, optional: true)
            .AddEnvironmentVariables(PrefijoVariablesEntorno)
            .Build();
}

public sealed class UsuarioPrueba
{
    public string CodigoUsuario { get; set; } = "USR_PRUEBA";
    public long UserIdLocal { get; set; } = 1;
    public string Dni { get; set; } = "00000000";
    public long AreaId { get; set; } = 1;
    public string AreaNombre { get; set; } = "AREA PRUEBA";
    public long AreaPadreId { get; set; } = 1;
    public string AreaDescripcionCorta { get; set; } = "PRB";
    public long CargoId { get; set; } = 1;
    public string GerenciaNombre { get; set; } = "GERENCIA PRUEBA";
    public string CodigoRolUsuario { get; set; } = "SOLAPR";
    public string Correo { get; set; } = "usuario.prueba@cofide.com.pe";
    public string Nombre { get; set; } = "Usuario";
    public string ApellidoPaterno { get; set; } = "Prueba";
    public string ApellidoMaterno { get; set; } = "Integracion";
}
