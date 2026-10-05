using System.Reflection;
using COFIDE.WFF.Architecture.Tests.Support;

namespace COFIDE.WFF.Architecture.Tests.Reglas;

/// <summary>
/// Reglas verificables del modelo de dominio (encapsulamiento, inmutabilidad y ausencia de
/// conceptos técnicos o de presentación).
/// </summary>
[Trait("Categoria", "DDD")]
public class ModeloDeDominioTests
{
    private static IReadOnlyList<Type> TiposDominio => Capas.Tipos(Capas.DomainAssembly);

    /// <summary>Entidades y agregados (excluye inputs/modelos de edición, que son parámetros de métodos).</summary>
    private static IEnumerable<Type> EntidadesYAgregados => TiposDominio
        .Where(t => t.IsClass && !t.IsAbstract)
        .Where(t => t.Namespace is { } ns
                    && (ns.Contains(".Aggregates") || ns.EndsWith(".Entities", StringComparison.Ordinal))
                    && !ns.Contains(".Inputs") && !ns.Contains(".Models")
                    && !ns.Contains(".Errors") && !ns.Contains(".Events"))
        .Where(t => !t.Name.EndsWith("Input", StringComparison.Ordinal)
                    && !t.Name.EndsWith("Model", StringComparison.Ordinal)
                    && !t.Name.EndsWith("Errors", StringComparison.Ordinal));

    [Fact, Trait("Hallazgo", "D6")]
    public void Entidades_y_agregados_no_exponen_setters_publicos()
    {
        var violaciones = EntidadesYAgregados
            .SelectMany(t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(p => p.TieneSetterPublicoMutable())
                .Select(p => $"{t.NombreCompleto()}.{p.Name}"));

        ArchitectureAssert.SinViolaciones(
            "D6 - Entidades sin setters públicos",
            violaciones,
            "Use 'private set' y modifique el estado mediante métodos del agregado que protejan sus invariantes.");
    }

    [Fact, Trait("Hallazgo", "Base")]
    public void Value_objects_son_inmutables()
    {
        var violaciones = TiposDominio
            .Where(t => t.HeredaDe("ValueObject"))
            .SelectMany(t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.TieneSetterPublicoMutable())
                .Select(p => $"{t.NombreCompleto()}.{p.Name}"));

        ArchitectureAssert.SinViolaciones(
            "Base - Value objects inmutables",
            violaciones,
            "Un value object no cambia: cree una nueva instancia en lugar de modificarla.");
    }

    [Fact, Trait("Hallazgo", "Base")]
    public void Eventos_de_dominio_son_inmutables()
    {
        var violaciones = TiposDominio
            .Where(t => t.IsClass && t.GetInterfaces().Any(i => i.Name == "IDomainEvent"))
            .SelectMany(t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.TieneSetterPublicoMutable())
                .Select(p => $"{t.NombreCompleto()}.{p.Name}"));

        ArchitectureAssert.SinViolaciones(
            "Base - Eventos de dominio inmutables",
            violaciones,
            "Un evento describe un hecho pasado: use records o propiedades init.");
    }

    [Fact, Trait("Hallazgo", "D5")]
    public void Domain_no_contiene_DTOs()
    {
        var violaciones = TiposDominio
            .Where(t => t.Name.EndsWith("Dto", StringComparison.OrdinalIgnoreCase)
                        || t.Name.EndsWith("DTO", StringComparison.Ordinal)
                        || t.Namespace?.Contains(".Dtos", StringComparison.OrdinalIgnoreCase) == true)
            .Select(t => t.NombreCompleto());

        ArchitectureAssert.SinViolaciones(
            "D5 - Domain sin DTOs",
            violaciones,
            "Los contratos externos van en Infraestructura; los conceptos de dominio se modelan como value objects.");
    }

    [Fact, Trait("Hallazgo", "D5")]
    public void Domain_no_contiene_conceptos_de_paginacion()
    {
        var violaciones = TiposDominio
            .Where(t => t.Name.StartsWith("Paged", StringComparison.Ordinal)
                        || t.Name.StartsWith("Paginado", StringComparison.Ordinal)
                        || t.Namespace?.Contains(".Paged", StringComparison.Ordinal) == true)
            .Select(t => t.NombreCompleto());

        ArchitectureAssert.SinViolaciones(
            "D5 - Domain sin paginación",
            violaciones,
            "La paginación es un concepto de consulta/presentación: muévala a Application/Common.");
    }

    [Fact, Trait("Hallazgo", "D4")]
    public void Domain_no_expone_banderas_de_persistencia()
    {
        string[] banderas = { "EsNuevo", "IsNew", "IsDirty", "EntityState", "EstadoPersistencia" };

        var violaciones = TiposDominio
            .SelectMany(t => t.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(p => banderas.Contains(p.Name))
                .Select(p => $"{t.NombreCompleto()}.{p.Name}"));

        ArchitectureAssert.SinViolaciones(
            "D4 - Domain sin banderas de persistencia",
            violaciones,
            "El tracking de altas/cambios es responsabilidad de EF Core, no del modelo de dominio.");
    }

    [Fact, Trait("Hallazgo", "D4")]
    public void Metodos_de_dominio_no_reciben_generadores_de_ids()
    {
        var violaciones = TiposDominio
            .SelectMany(t => t.MetodosDeclarados()
                .Where(m => m.IsPublic)
                .Where(m => m.GetParameters().Any(p =>
                    p.ParameterType.IsGenericType
                    && p.ParameterType.GetGenericTypeDefinition() == typeof(Func<>)
                    && (p.ParameterType.GetGenericArguments()[0] == typeof(long)
                        || p.ParameterType.GetGenericArguments()[0] == typeof(int))))
                .Select(m => $"{t.NombreCompleto()}.{m.Name}"));

        ArchitectureAssert.SinViolaciones(
            "D4 - Sin Func<long> generadores de Id",
            violaciones,
            "Deje que la persistencia asigne los Ids (secuencias/HiLo en EF Core) en lugar de pasarlos al dominio.");
    }

    [Fact, Trait("Hallazgo", "Base")]
    public void Entidades_de_agregados_no_tienen_constructor_publico_vacio()
    {
        // Un agregado y sus entidades se crean por factory o método Crear para garantizar un estado válido.
        var entidades = TiposDominio
            .Where(t => t.IsClass && !t.IsAbstract && t.Namespace?.Contains(".Aggregates.") == true)
            .Where(t => t.Namespace!.EndsWith("Aggregate", StringComparison.Ordinal))
            .Where(t => t.HeredaDe("BaseEntity"));

        var violaciones = entidades
            .Where(t => t.GetConstructor(Type.EmptyTypes) is { IsPublic: true })
            .Select(t => t.NombreCompleto());

        ArchitectureAssert.SinViolaciones(
            "Base - Agregados sin constructor público vacío",
            violaciones,
            "Use un constructor privado/protegido para EF Core y exponga un método de creación que valide invariantes.");
    }
}
