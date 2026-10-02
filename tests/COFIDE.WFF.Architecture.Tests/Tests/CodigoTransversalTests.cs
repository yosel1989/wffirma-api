using System.Reflection;
using COFIDE.WFF.Architecture.Tests.Support;

namespace COFIDE.WFF.Architecture.Tests.Reglas;

/// <summary>Código transversal: modelo de errores único, configuración técnica compartida y helpers no duplicados.</summary>
[Trait("Categoria", "Transversal")]
public class CodigoTransversalTests
{
    [Fact, Trait("Hallazgo", "C1")]
    public void Existe_un_unico_tipo_de_error()
    {
        // Un "tipo de error" = clase/record con Código y Descripción cuyo nombre termina en "Error".
        static bool EsTipoError(Type t)
        {
            if (!t.Name.EndsWith("Error", StringComparison.Ordinal) || (t.IsAbstract && t.IsSealed)) return false;
            var props = t.GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => p.Name).ToHashSet();
            return (props.Contains("Code") || props.Contains("Codigo"))
                   && (props.Contains("Description") || props.Contains("Descripcion"));
        }

        var tipos = Capas.Todas.SelectMany(a => Capas.Tipos(a)).Where(EsTipoError).ToList();
        var violaciones = tipos.Count > 1 ? tipos.Select(t => t.NombreCompleto()) : Enumerable.Empty<string>();

        ArchitectureAssert.SinViolaciones(
            "C1 - Un solo tipo Error",
            violaciones,
            "Unifique DomainError/ApplicationError en SharedKernel.Error (con TipoError).");
    }

    [Fact, Trait("Hallazgo", "C1")]
    public void Existe_una_unica_excepcion_base_de_la_solucion()
    {
        var bases = Capas.Todas
            .SelectMany(a => Capas.Tipos(a))
            .Where(t => t.IsAbstract && !t.IsSealed && typeof(Exception).IsAssignableFrom(t))
            .ToList();

        var violaciones = bases.Count > 1 ? bases.Select(t => t.NombreCompleto()) : Enumerable.Empty<string>();

        ArchitectureAssert.SinViolaciones(
            "C1 - Una sola excepción base",
            violaciones,
            "Use SharedKernel.AppException como base y capture solo esa en el middleware.");
    }

    [Fact, Trait("Hallazgo", "C2")]
    public void Configuracion_de_logging_en_un_solo_lugar()
    {
        var patron = Codigo.Patron(@"new\s+LoggerConfiguration\s*\(");

        var proyectos = RutasSolucion.ProyectosSrc()
            .Where(p => RutasSolucion.ArchivosCs(p).Any(a => a.LineasDeCodigo().Any(l => patron.IsMatch(l.Texto))))
            .ToList();

        var violaciones = proyectos.Count > 1
            ? proyectos.Select(p => $"{p} configura Serilog por su cuenta")
            : Enumerable.Empty<string>();

        ArchitectureAssert.SinViolaciones(
            "C2 - Logging centralizado",
            violaciones,
            "Mueva la configuración a CrossCutting (AddWffSerilog) y llámela desde Api y Worker.");
    }

    [Fact, Trait("Hallazgo", "C2")]
    public void Application_no_contiene_piezas_tecnicas_de_correlacion()
    {
        var violaciones = Capas.Tipos(Capas.ApplicationAssembly)
            .Where(t => t.Name.Contains("Correlation", StringComparison.Ordinal))
            .Select(t => t.NombreCompleto());

        ArchitectureAssert.SinViolaciones(
            "C2 - Correlación fuera de Application",
            violaciones,
            "La correlación es técnica: muévala a CrossCutting/Correlation.");
    }

    [Fact, Trait("Hallazgo", "C3")]
    public void Metodos_privados_no_estan_copiados_entre_clases()
    {
        // Misma firma (nombre + parámetros + retorno) en métodos privados de clases distintas = posible copia.
        var firmas = Capas.Todas
            .SelectMany(a => Capas.Tipos(a))
            .Where(t => t.IsClass)
            .SelectMany(t => t.MetodosDeclarados()
                .Where(m => m.IsPrivate)
                .Select(m => (Tipo: t, Firma: Firma(m))))
            .GroupBy(x => x.Firma, StringComparer.Ordinal)
            .Where(g => g.Select(x => x.Tipo).Distinct().Count() > 1);

        var violaciones = firmas.Select(g =>
            $"{g.Key} en: {string.Join(", ", g.Select(x => x.Tipo.Name).Distinct().OrderBy(n => n))}");

        ArchitectureAssert.SinViolaciones(
            "C3 - Helpers duplicados",
            violaciones,
            "Consolide el método en un único helper dentro de la capa que corresponde (no en un proyecto 'Utils').");
    }

    private static string Firma(MethodInfo m) =>
        $"{m.ReturnType.Name} {m.Name}({string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name))})";
}
