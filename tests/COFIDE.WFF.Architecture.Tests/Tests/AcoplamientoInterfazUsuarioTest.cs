using System.Reflection;
using COFIDE.WFF.Architecture.Tests.Support;

namespace COFIDE.WFF.Architecture.Tests.Reglas;

/// <summary>El backend no debe conocer estilos, rutas del front, controles visuales ni decisiones de navegación.</summary>
[Trait("Categoria", "UI")]
public class AcoplamientoInterfazUsuarioTest
{
    private static IEnumerable<ArchivoFuente> Nucleo =>
        RutasSolucion.ArchivosCs(Capas.Domain).Concat(RutasSolucion.ArchivosCs(Capas.Application));

    [Fact, Trait("Hallazgo", "U1-U4")]
    public void Nucleo_no_contiene_HTML_ni_estilos()
    {
        var patron = Codigo.Patron(@"style\s*=\s*\\?['""]|<a\s+href|<br\s*/?>|<(div|span|table|strong|td|tr|p)[\s>]", ignorarMayusculas: true);

        ArchitectureAssert.SinViolaciones(
            "U1-U4 - Núcleo sin HTML",
            Codigo.Coincidencias(Nucleo, patron),
            "El HTML y los estilos van en las plantillas (FormatoNotificacion) o en el adaptador; el núcleo solo envía datos.");
    }

    [Fact, Trait("Hallazgo", "U1-H7")]
    public void Nucleo_no_arma_URLs_del_front_ni_del_backend()
    {
        // Interpolaciones del tipo $"{baseFrontUrl}/ruta" o rutas fijas del backend como "/API-WFF/".
        var patron = Codigo.Patron(@"Url\w*\}\s*/|Url\w*\.TrimEnd\([^)]*\)\}\s*/|""/API-[A-Z]+/", ignorarMayusculas: true);

        ArchitectureAssert.SinViolaciones(
            "U1-H7 - URLs fuera del núcleo",
            Codigo.Coincidencias(Nucleo, patron),
            "Obtenga las URLs de IUrlProvider con métodos semánticos (UrlDetalleSolicitud, UrlDescargaPublica) y rutas en configuración.");
    }

    [Fact, Trait("Hallazgo", "U2")]
    public void Nombres_no_hacen_referencia_a_controles_visuales()
    {
        string[] controles = { "Combo", "Dropdown", "Grilla", "Grid", "Modal", "Popup", "Checkbox" };

        bool Menciona(string nombre) => controles.Any(c => nombre.Contains(c, StringComparison.Ordinal));

        var violaciones = new List<string>();
        foreach (var assembly in new[] { Capas.DomainAssembly, Capas.ApplicationAssembly, Capas.InfrastructureAssembly, Capas.ApiAssembly })
            foreach (var tipo in Capas.Tipos(assembly))
            {
                if (Menciona(tipo.Name))
                    violaciones.Add(tipo.NombreCompleto());

                violaciones.AddRange(tipo.MetodosDeclarados()
                    .Where(m => Menciona(m.Name))
                    .Select(m => $"{tipo.NombreCompleto()}.{m.Name}()"));
            }

        ArchitectureAssert.SinViolaciones(
            "U2 - Sin nombres de controles visuales",
            violaciones,
            "Nombre por el concepto de negocio (Catalogo, Opciones, Listado), no por el control que lo muestra.");
    }

    [Fact, Trait("Hallazgo", "U3")]
    public void Nucleo_no_decide_la_navegacion_del_front()
    {
        string[] navegacion = { "Redirig", "Redirecc", "Redirect", "Navegar", "Navigate" };

        var violaciones = new[] { Capas.DomainAssembly, Capas.ApplicationAssembly }
            .SelectMany(a => Capas.Tipos(a))
            .SelectMany(t => t.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Where(m => m is MethodInfo { IsSpecialName: false } or PropertyInfo)
                .Where(m => navegacion.Any(n => m.Name.Contains(n, StringComparison.OrdinalIgnoreCase)))
                .Select(m => $"{t.NombreCompleto()}.{m.Name}"));

        ArchitectureAssert.SinViolaciones(
            "U3 - Navegación en el front",
            violaciones,
            "Exponga un hecho de negocio (p. ej. UsuarioEsAprobadorActual) y deje que el front decida si redirige.");
    }
}
