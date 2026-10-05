using System.Reflection;
using System.Text.RegularExpressions;
using COFIDE.WFF.Architecture.Tests.Support;

namespace COFIDE.WFF.Architecture.Tests.Reglas;

/// <summary>
/// Desarrollo orientado a dominio: la lógica de negocio debe vivir en Domain.
///
/// Que una regla de negocio esté "en el lugar correcto" no se puede demostrar al 100 % con
/// código; estas pruebas combinan:
///   - Garantías estructurales (el dominio es el único que puede cambiar su estado y lanzar sus errores).
///   - Señales de lógica fuera del dominio (servicios con reglas, handlers con muchas decisiones).
/// Las señales deben confirmarse en la revisión experta antes de corregirse.
/// </summary>
[Trait("Categoria", "DDD")]
public class LogicaDeNegocioEnDominioTests
{
    /// <summary>Máximo de decisiones (if/else if/switch/case/?:/&&/||/when) aceptable en un handler o servicio de Application.</summary>
    private const int MaximoDecisionesPorArchivo = 12;

    private static IReadOnlyList<Type> TiposDominio => Capas.Tipos(Capas.DomainAssembly);

    /// <summary>Raíces de agregado: clase con el mismo nombre que su carpeta "XxxAggregate".</summary>
    private static IEnumerable<Type> RaicesDeAgregado => TiposDominio
        .Where(t => t.IsClass && !t.IsAbstract && t.Namespace is not null)
        .Where(t => t.Namespace!.EndsWith($".{t.Name}Aggregate", StringComparison.Ordinal));

    private static IReadOnlyList<Type> EntidadesDeDominio => TiposDominio
        .Where(t => t.IsClass && !t.IsAbstract && t.HeredaDe("BaseEntity"))
        .ToList();

    // ---------------------------------------------------------------------------------
    // Garantías estructurales
    // ---------------------------------------------------------------------------------

    [Fact, Trait("Hallazgo", "Base")]
    public void Existen_raices_de_agregado_en_el_dominio()
    {
        var violaciones = RaicesDeAgregado.Any()
            ? Enumerable.Empty<string>()
            : new[] { "No se encontraron raíces de agregado (clase Xxx dentro de un namespace ...XxxAggregate)" };

        ArchitectureAssert.SinViolaciones(
            "Base - Agregados identificables",
            violaciones,
            "Mantenga la convención Aggregates/XxxAggregate/Xxx.cs para que las reglas puedan identificar las raíces.");
    }

    [Fact, Trait("Hallazgo", "Base")]
    public void Raices_de_agregado_tienen_comportamiento_no_son_anemicas()
    {
        // Un agregado anémico solo tiene propiedades: la lógica termina en servicios de Application.
        var violaciones = RaicesDeAgregado
            .Where(t => !t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Any(m => !m.IsSpecialName && !m.EsMiembroGeneradoPorCompilador()
                          && m.Name is not ("ToString" or "Equals" or "GetHashCode")))
            .Select(t => t.NombreCompleto());

        ArchitectureAssert.SinViolaciones(
            "Base - Agregados con comportamiento",
            violaciones,
            "Mueva las reglas que modifican el agregado a métodos del propio agregado.");
    }

    [Fact, Trait("Hallazgo", "Base")]
    public void Solo_el_dominio_lanza_excepciones_de_dominio_desde_Infrastructure_y_Api()
    {
        var archivos = RutasSolucion.ArchivosCs(Capas.Infrastructure).Concat(RutasSolucion.ArchivosCs(Capas.Api));
        var patron = Codigo.Patron(@"throw\s+new\s+Domain\w*Exception\b");

        ArchitectureAssert.SinViolaciones(
            "Base - DomainException solo en Domain (adaptadores)",
            Codigo.Coincidencias(archivos, patron),
            "Si un adaptador necesita lanzar un error de negocio, la regla pertenece al dominio.");
    }

    [Fact, Trait("Hallazgo", "D2")]
    public void Adaptadores_no_usan_los_catalogos_de_errores_de_negocio()
    {
        // Si Infrastructure/Api referencian SolicitudErrors o PlantillaErrors, están validando reglas de negocio.
        var namespacesDeErrores = TiposDominio
            .Select(t => t.Namespace)
            .OfType<string>()
            .Where(ns => ns.EndsWith(".Errors", StringComparison.Ordinal) && !ns.Contains(".Common.", StringComparison.Ordinal))
            .Distinct()
            .ToArray();

        var violaciones = Dependencias.Prohibidas(Capas.InfrastructureAssembly, namespacesDeErrores)
            .Concat(Dependencias.Prohibidas(Capas.ApiAssembly, namespacesDeErrores));

        ArchitectureAssert.SinViolaciones(
            "D2 - Errores de negocio solo en el núcleo",
            violaciones,
            "Los adaptadores extraen/persisten datos; la validación de negocio y sus errores pertenecen al dominio.");
    }

    [Fact, Trait("Hallazgo", "D2")]
    public void Adaptadores_no_devuelven_mensajes_de_negocio_como_texto()
    {
        // Métodos privados que devuelven string y se llaman Validar* suelen ser reglas de negocio con mensaje.
        var violaciones = Capas.Tipos(Capas.InfrastructureAssembly)
            .SelectMany(t => t.MetodosDeclarados()
                .Where(m => m.Name.StartsWith("Validar", StringComparison.Ordinal) && m.ReturnType == typeof(string))
                .Select(m => $"{t.NombreCompleto()}.{m.Name}"));

        ArchitectureAssert.SinViolaciones(
            "D2 - Validaciones de negocio fuera de adaptadores",
            violaciones,
            "Separe la extracción técnica (Infraestructura) de la política de negocio (servicio de dominio con errores tipados).");
    }

    [Fact, Trait("Hallazgo", "D6-A2")]
    public void Application_no_instancia_entidades_de_dominio_con_new()
    {
        // Las entidades se crean por factories o métodos del agregado, que garantizan invariantes.
        var nombres = EntidadesDeDominio.Select(t => Regex.Escape(ReflexionExtensions.NormalizarNombreTipo(t.Name))).Distinct().ToList();
        if (nombres.Count == 0)
            return;

        var alternativas = string.Join("|", nombres);
        var patron = Codigo.Patron(@"\bnew\s+(" + alternativas + @")\s*[({]");

        ArchitectureAssert.SinViolaciones(
            "D6-A2 - Entidades creadas por el dominio",
            Codigo.Coincidencias(RutasSolucion.ArchivosCs(Capas.Application), patron),
            "Cree las entidades con su factory o con un método del agregado raíz (p. ej. solicitud.AgregarDocumento(...)).");
    }

    [Fact, Trait("Hallazgo", "Base")]
    public void Servicios_de_dominio_viven_en_Domain()
    {
        var violaciones = new[] { Capas.ApplicationAssembly, Capas.InfrastructureAssembly, Capas.ApiAssembly }
            .SelectMany(a => Capas.Tipos(a))
            .Where(t => t.Name.Contains("DomainService", StringComparison.Ordinal))
            .Select(t => t.NombreCompleto());

        ArchitectureAssert.SinViolaciones(
            "Base - Servicios de dominio en Domain",
            violaciones,
            "Un servicio de dominio contiene reglas de negocio: su interfaz y su implementación van en Domain.");
    }

    // ---------------------------------------------------------------------------------
    // Señales de lógica de negocio fuera del dominio (confirmar con revisión experta)
    // ---------------------------------------------------------------------------------

    [Fact, Trait("Hallazgo", "A2"), Trait("Tipo", "Senal")]
    public void Application_no_contiene_servicios_con_logica_propia()
    {
        string[] sufijos = { "Service", "Factory", "Manager", "Helper", "Calculator", "Policy", "Processor" };

        var violaciones = Capas.Tipos(Capas.ApplicationAssembly)
            .Where(t => t.EsClaseConcreta() && !t.EsHandlerMediatR())
            .Where(t => sufijos.Any(s => t.Name.EndsWith(s, StringComparison.Ordinal)))
            .Select(t => $"{t.NombreCompleto()} ({t.MetodosDeclarados().Count(m => m.IsPublic)} métodos públicos)");

        ArchitectureAssert.SinViolaciones(
            "A2 - Servicios en Application",
            violaciones,
            "Si el servicio decide reglas de negocio, muévalo a un servicio de dominio; si es técnico, detrás de un puerto en Infraestructura; si solo orquesta, intégrelo al Handler.");
    }

    [Fact, Trait("Hallazgo", "A2"), Trait("Tipo", "Senal")]
    public void Handlers_y_servicios_de_Application_no_concentran_decisiones()
    {
        var decision = Codigo.Patron(@"\bif\s*\(|\bcase\s+|\bswitch\b|\?\s*[^?.:]+\s*:|&&|\|\||\bwhen\b");

        var violaciones = RutasSolucion.ArchivosCs(Capas.Application)
            .Where(a => a.NombreArchivo.EndsWith("Handler.cs", StringComparison.Ordinal)
                        || a.NombreArchivo.EndsWith("Service.cs", StringComparison.Ordinal)
                        || a.NombreArchivo.EndsWith("UseCase.cs", StringComparison.Ordinal))
            .Select(a => (Archivo: a, Decisiones: a.LineasDeCodigo().Sum(l => decision.Matches(l.Texto).Count)))
            .Where(x => x.Decisiones > MaximoDecisionesPorArchivo)
            .Select(x => $"{x.Archivo.RutaRelativa}: {x.Decisiones} decisiones");

        ArchitectureAssert.SinViolaciones(
            "A2 - Complejidad en Application",
            violaciones,
            $"Más de {MaximoDecisionesPorArchivo} decisiones sugiere reglas de negocio en la orquestación: revise si pertenecen al agregado o a un servicio de dominio.");
    }

    [Fact, Trait("Hallazgo", "A2"), Trait("Tipo", "Senal")]
    public void Application_no_consulta_el_estado_interno_de_las_entidades_para_decidir()
    {
        // Patrones como "if (solicitud.IdEstadoSolicitud == ...)" o "if (x.Estado ...)" en Application
        // indican que la decisión debería ser un método del agregado (p. ej. solicitud.PuedeAprobarse()).
        var patron = Codigo.Patron(@"\bif\s*\(.*\.(Id)?(Estado|Tipo)\w*\s*(==|!=|is)\s*");

        ArchitectureAssert.SinViolaciones(
            "A2 - Decisiones sobre estado en Application",
            Codigo.Coincidencias(RutasSolucion.ArchivosCs(Capas.Application), patron),
            "Exprese la decisión como un método del agregado (Tell, don't ask).");
    }
}
