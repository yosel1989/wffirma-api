using System.Reflection;
using NetArchTest.Rules;

namespace COFIDE.WFF.Architecture.Tests.Support;

/// <summary>Análisis de dependencias a nivel de IL (incluye cuerpos de métodos) con NetArchTest.</summary>
public static class Dependencias
{
    /// <summary>
    /// Devuelve "Tipo ⇒ dependencia" por cada tipo del ensamblado (opcionalmente filtrado por namespace)
    /// que usa alguno de los namespaces prohibidos.
    /// </summary>
    public static IEnumerable<string> Prohibidas(Assembly assembly, IEnumerable<string> prohibidas, string? soloNamespace = null)
    {
        var violaciones = new List<string>();

        foreach (var dependencia in prohibidas)
        {
            var tipos = Types.InAssembly(assembly);
            var resultado = soloNamespace is null
                ? tipos.ShouldNot().HaveDependencyOn(dependencia).GetResult()
                : tipos.That().ResideInNamespace(soloNamespace).ShouldNot().HaveDependencyOn(dependencia).GetResult();

            if (resultado.IsSuccessful)
                continue;

            IEnumerable<string> fallidos = resultado.FailingTypeNames ?? Enumerable.Empty<string>();
            violaciones.AddRange(fallidos
                .Select(ReflexionExtensions.NormalizarNombreTipo)
                .Distinct()
                .Select(t => $"{t} ⇒ {dependencia}"));
        }

        return violaciones;
    }
}
