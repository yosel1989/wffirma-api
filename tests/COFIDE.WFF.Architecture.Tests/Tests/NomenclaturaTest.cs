using System.Reflection;
using System.Text.RegularExpressions;
using COFIDE.WFF.Architecture.Tests.Support;

namespace COFIDE.WFF.Architecture.Tests.Reglas;

/// <summary>Convenciones de nombres verificables automáticamente.</summary>
[Trait("Categoria", "Nomenclatura")]
public class NomenclaturaTest
{
    [Fact, Trait("Hallazgo", "N1")]
    public void Archivos_sin_espacios_en_el_nombre()
    {
        var violaciones = RutasSolucion.ArchivosCs()
            .Where(a => a.NombreArchivo.Contains(' '))
            .Select(a => $"'{a.RutaRelativa}'");

        ArchitectureAssert.SinViolaciones(
            "N1 - Nombres de archivo",
            violaciones,
            "Renombre con 'git mv' quitando los espacios.");
    }

    [Fact, Trait("Hallazgo", "Base")]
    public void Nombre_de_archivo_coincide_con_el_tipo_principal()
    {
        // Solo archivos con un único tipo de nivel superior.
        var declaracion = Codigo.Patron(@"^\s*(?:public|internal)\s+(?:(?:sealed|static|abstract|partial|readonly)\s+)*(?:record\s+(?:class\s+|struct\s+)?|class\s+|interface\s+|struct\s+|enum\s+)([A-Z]\w*)");

        var violaciones = new List<string>();
        foreach (var archivo in RutasSolucion.ArchivosCs())
        {
            var tipos = archivo.LineasDeCodigo()
                .Select(l => declaracion.Match(l.Texto))
                .Where(m => m.Success)
                .Select(m => m.Groups[1].Value)
                .Distinct()
                .ToList();

            var nombre = Path.GetFileNameWithoutExtension(archivo.NombreArchivo).Trim();
            if (tipos.Count == 1 && tipos[0] != nombre && !nombre.StartsWith(tipos[0] + ".", StringComparison.Ordinal))
                violaciones.Add($"{archivo.RutaRelativa} declara {tipos[0]}");
        }

        ArchitectureAssert.SinViolaciones(
            "Base - Archivo = tipo",
            violaciones,
            "Haga coincidir el nombre del archivo con el del tipo que declara.");
    }

    [Fact, Trait("Hallazgo", "N7")]
    public void Metodos_con_sufijo_Async_devuelven_Task()
    {
        var violaciones = Capas.Todas
            .SelectMany(a => Capas.Tipos(a))
            .SelectMany(t => t.MetodosDeclarados()
                .Where(m => m.Name.EndsWith("Async", StringComparison.Ordinal))
                .Where(m => !m.ReturnType.EsTipoAwaitable())
                .Select(m => $"{t.NombreCompleto()}.{m.Name} devuelve {m.ReturnType.Name}"));

        ArchitectureAssert.SinViolaciones(
            "N7 - Sufijo Async",
            violaciones,
            "Quite el sufijo Async de los métodos síncronos (o hágalos realmente asíncronos).");
    }

    [Fact, Trait("Hallazgo", "N7")]
    public void Metodos_sin_guion_bajo_en_el_nombre()
    {
        var violaciones = Capas.Todas
            .SelectMany(a => Capas.Tipos(a))
            .Where(t => t.Namespace?.Contains(".Migrations") != true)
            .SelectMany(t => t.MetodosDeclarados()
                .Where(m => m.Name.Contains('_') && !m.Name.Contains('.')) // excluye implementaciones explícitas
                .Select(m => $"{t.NombreCompleto()}.{m.Name}"));

        ArchitectureAssert.SinViolaciones(
            "N7 - Métodos en PascalCase",
            violaciones,
            "Use PascalCase sin guiones bajos (p. ej. ValidarFirmaExternaAsync).");
    }

    [Fact, Trait("Hallazgo", "N2")]
    public void Sin_errores_ortograficos_conocidos_en_nombres()
    {
        // Diccionario ampliable con errores frecuentes detectados en la revisión.
        var errores = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Hander"] = "Handler",
            ["Solitud"] = "Solicitud",
            ["Aproval"] = "Approval / Aprobacion",
            ["coemntario"] = "comentario",
            ["Wokflow"] = "Workflow",
            ["Pageds"] = "Paged / Paginacion",
        };

        var violaciones = new List<string>();

        foreach (var archivo in RutasSolucion.ArchivosCs())
        {
            foreach (var (mal, bien) in errores)
            {
                if (archivo.NombreArchivo.Contains(mal, StringComparison.Ordinal))
                    violaciones.Add($"{archivo.RutaRelativa} (archivo): '{mal}' → '{bien}'");

                // Una sola entrada por archivo y palabra (la primera aparición) para no duplicar cada uso.
                var primera = archivo.LineasDeCodigo()
                    .FirstOrDefault(l => Regex.IsMatch(l.Texto, $@"\b\w*{mal}\w*\b"));
                if (primera.Texto is not null)
                    violaciones.Add($"{archivo.RutaRelativa}:{primera.Numero}: '{mal}' → '{bien}'");
            }
        }

        ArchitectureAssert.SinViolaciones(
            "N2 - Ortografía en nombres",
            violaciones.Distinct(),
            "Corrija el nombre con refactor/rename del IDE.");
    }

    [Fact, Trait("Categoria", "Transversal"), Trait("Hallazgo", "N5-C2")]
    public void Nombres_de_tipos_no_se_repiten_entre_proyectos_o_namespaces()
    {
        // Se analiza el código fuente para incluir también al Worker (no referenciado por este proyecto).
        var declaracion = Codigo.Patron(@"\b(?:record\s+(?:class\s+|struct\s+)?|class\s+|interface\s+|struct\s+|enum\s+)([A-Z]\w*)");
        var espacio = Codigo.Patron(@"^\s*namespace\s+([\w.]+)");

        // Nombres que por convención se repiten en cada proyecto.
        var permitidos = new HashSet<string>(StringComparer.Ordinal) { "DependencyInjection", "Program", "GlobalUsings", "AssemblyInfo" };

        var declaraciones = new List<(string Nombre, string Ubicacion)>();
        foreach (var proyecto in RutasSolucion.ProyectosSrc())
            foreach (var archivo in RutasSolucion.ArchivosCs(proyecto))
            {
                var lineas = archivo.LineasDeCodigo();
                var ns = lineas.Select(l => espacio.Match(l.Texto)).FirstOrDefault(m => m.Success)?.Groups[1].Value ?? "(global)";

                foreach (var (_, texto) in lineas)
                    foreach (Match m in declaracion.Matches(texto))
                        declaraciones.Add((m.Groups[1].Value, $"{ns}"));
            }

        var violaciones = declaraciones
            .Where(d => !permitidos.Contains(d.Nombre))
            .GroupBy(d => d.Nombre, StringComparer.Ordinal)
            .Select(g => (Nombre: g.Key, Ubicaciones: g.Select(x => x.Ubicacion).Distinct(StringComparer.Ordinal).OrderBy(x => x).ToList()))
            .Where(g => g.Ubicaciones.Count > 1)
            .Select(g => $"{g.Nombre}: {string.Join(" | ", g.Ubicaciones)}");

        ArchitectureAssert.SinViolaciones(
            "N5-C2 - Tipos con nombre duplicado",
            violaciones,
            "Renombre las colisiones (entidad vs. value object) y centralice los tipos técnicos duplicados en CrossCutting/SharedKernel.");
    }
}
