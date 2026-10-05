using COFIDE.WFF.Architecture.Tests.Support;

namespace COFIDE.WFF.Architecture.Tests.Reglas;

/// <summary>
/// Lista blanca de referencias entre proyectos: cualquier referencia que no esté permitida
/// explícitamente falla (a diferencia de las reglas de prohibición, esto también detecta
/// referencias nuevas que nadie previó). Incluye detección de ciclos.
/// </summary>
[Trait("Categoria", "Hexagonal")]
public class MatrizDeReferenciasTests
{
    /// <summary>
    /// Referencias permitidas por proyecto. Domain es el centro; las flechas solo apuntan hacia adentro.
    /// Api y Worker son hosts (composition root) y pueden referenciar Infrastructure para registrar adaptadores.
    /// </summary>
    private static readonly Dictionary<string, string[]> Permitidas = new(StringComparer.Ordinal)
    {
        [Capas.Application] = new[] { Capas.Domain, },
        [Capas.Infrastructure] = new[] { Capas.Application, Capas.Domain},
        [Capas.Api] = new[] { Capas.Application, Capas.Infrastructure, Capas.Domain},
        [Capas.Worker] = new[] { Capas.Application, Capas.Infrastructure, Capas.Domain},
    };

    [Fact, Trait("Hallazgo", "Base")]
    public void Cada_proyecto_solo_tiene_referencias_permitidas()
    {
        var violaciones = new List<string>();

        foreach (var proyecto in RutasSolucion.ProyectosSrc())
        {
            var csproj = ProyectoMsBuild.Cargar(proyecto);

            if (!Permitidas.TryGetValue(proyecto, out var permitidas))
            {
                violaciones.Add($"{proyecto}: proyecto nuevo sin reglas de referencia; agréguelo a la matriz de MatrizDeReferenciasTests");
                continue;
            }

            violaciones.AddRange(csproj.ReferenciasProyecto
                .Where(r => !permitidas.Contains(r))
                .Select(r => $"{proyecto} → {r} (no permitida)"));
        }

        ArchitectureAssert.SinViolaciones(
            "Base - Matriz de referencias",
            violaciones,
            "Las referencias solo pueden apuntar hacia el núcleo: Api/Worker → Infrastructure → Application → Domain.");
    }

    [Fact, Trait("Hallazgo", "Base")]
    public void No_existen_referencias_circulares()
    {
        var grafo = RutasSolucion.ProyectosSrc()
            .ToDictionary(p => p, p => ProyectoMsBuild.Cargar(p).ReferenciasProyecto.ToList(), StringComparer.Ordinal);

        var ciclos = new List<string>();
        foreach (var inicio in grafo.Keys)
            BuscarCiclos(inicio, grafo, new List<string>(), ciclos);

        ArchitectureAssert.SinViolaciones(
            "Base - Sin ciclos de referencias",
            ciclos.Distinct(),
            "Elimine la referencia que cierra el ciclo; extraiga lo compartido a un proyecto inferior.");
    }

   

    private static void BuscarCiclos(string actual, Dictionary<string, List<string>> grafo, List<string> camino, List<string> ciclos)
    {
        if (camino.Contains(actual))
        {
            var desde = camino.IndexOf(actual);
            ciclos.Add(string.Join(" → ", camino.Skip(desde).Append(actual)));
            return;
        }

        if (!grafo.TryGetValue(actual, out var referencias))
            return;

        camino.Add(actual);
        foreach (var r in referencias)
            BuscarCiclos(r, grafo, camino, ciclos);
        camino.RemoveAt(camino.Count - 1);
    }
}
