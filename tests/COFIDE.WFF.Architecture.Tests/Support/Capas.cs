using System.Reflection;

namespace COFIDE.WFF.Architecture.Tests.Support;

/// <summary>
/// Nombres y ensamblados de cada capa de la solución.
/// </summary>
public static class Capas
{
    public const string Prefijo = "COFIDE.WFF";

    public const string Domain = "COFIDE.WFF.Domain";
    public const string Application = "COFIDE.WFF.Application";
    public const string Infrastructure = "COFIDE.WFF.Infrastructure";
    public const string Api = "COFIDE.WFF.Api";
    public const string Worker = "COFIDE.WFF.Worker";
    
    private static readonly Lazy<Assembly> _domain = new(() => Assembly.Load(Domain));
    private static readonly Lazy<Assembly> _application = new(() => Assembly.Load(Application));
    private static readonly Lazy<Assembly> _infrastructure = new(() => Assembly.Load(Infrastructure));
    private static readonly Lazy<Assembly> _api = new(() => Assembly.Load(Api));

    public static Assembly DomainAssembly => _domain.Value;
    public static Assembly ApplicationAssembly => _application.Value;
    public static Assembly InfrastructureAssembly => _infrastructure.Value;
    public static Assembly ApiAssembly => _api.Value;

    public static IReadOnlyList<Assembly> Todas =>
        new[] { DomainAssembly, ApplicationAssembly, InfrastructureAssembly, ApiAssembly };

    /// <summary>Tipos escritos por el equipo (sin tipos generados por el compilador).</summary>
    public static IReadOnlyList<Type> Tipos(Assembly assembly) =>
        assembly.TiposCargables().Where(t => !t.EsGeneradoPorCompilador()).ToList();
}
