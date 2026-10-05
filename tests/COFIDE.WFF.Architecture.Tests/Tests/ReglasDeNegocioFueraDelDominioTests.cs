using COFIDE.WFF.Architecture.Tests.Support;

namespace COFIDE.WFF.Architecture.Tests.Reglas;

/// <summary>
/// Señales automáticas de reglas de negocio implementadas fuera del dominio.
/// No reemplazan la revisión experta (ver README), pero detectan los patrones más comunes.
/// </summary>
[Trait("Categoria", "DDD")]
public class ReglasDeNegocioFueraDelDominioTests
{
    private static IEnumerable<ArchivoFuente> Repositorios =>
        RutasSolucion.ArchivosCs(Capas.Infrastructure)
            .Where(a => a.NombreArchivo.EndsWith("Repository.cs", StringComparison.Ordinal));

    [Fact, Trait("Hallazgo", "D1")]
    public void Repositorios_no_usan_roles_escritos_como_texto()
    {
        // Literales cortos en mayúsculas comparados con variables de rol/perfil.
        var patron = Codigo.Patron(@"(rol|perfil)\w*\s*==\s*""[A-Z]{3,}""|""[A-Z]{3,}""\s*==\s*\w*(rol|perfil)", ignorarMayusculas: true);

        ArchitectureAssert.SinViolaciones(
            "D1 - Roles sin literales en repositorios",
            Codigo.Coincidencias(Repositorios, patron),
            "Use el enum RolUsuario y especificaciones de dominio en lugar de comparar cadenas.");
    }

    [Fact, Trait("Hallazgo", "D1")]
    public void Repositorios_no_comparan_estados_con_numeros_magicos()
    {
        var patron = Codigo.Patron(@"\b(Id)?(Estado|Tipo|Orden)\w*\s*(==|!=)\s*\d+\b");

        ArchitectureAssert.SinViolaciones(
            "D1 - Estados sin números mágicos",
            Codigo.Coincidencias(Repositorios, patron),
            "Use los value objects (EstadoSolicitud.EnCurso.Codigo) o especificaciones del dominio.");
    }

    [Fact, Trait("Hallazgo", "D8")]
    public void Handlers_y_servicios_no_usan_codigos_de_catalogo_como_texto()
    {
        // Constantes en MAYUSCULAS_CON_GUION_BAJO fuera de clases de errores o de constantes.
        var archivos = RutasSolucion.ArchivosCs(Capas.Application)
            .Where(a => !a.NombreArchivo.EndsWith("Errors.cs", StringComparison.Ordinal)
                        && !a.NombreArchivo.Contains("Constant", StringComparison.Ordinal)
                        && !a.NombreArchivo.StartsWith("Codigos", StringComparison.Ordinal));

        var patron = Codigo.Patron(@"""[A-Z]{2,}(?:_[A-Z0-9]+)+""");

        ArchitectureAssert.SinViolaciones(
            "D8 - Códigos de catálogo centralizados",
            Codigo.Coincidencias(archivos, patron),
            "Centralice los códigos en clases de constantes (CodigosParametro, CodigosPlantillaCorreo) o en value objects.");
    }

    [Fact, Trait("Hallazgo", "D7")]
    public void Nucleo_no_usa_el_reloj_del_sistema()
    {
        var archivos = RutasSolucion.ArchivosCs(Capas.Domain).Concat(RutasSolucion.ArchivosCs(Capas.Application));
        var patron = Codigo.Patron(@"\bDateTime(Offset)?\.(Now|UtcNow|Today)\b");

        ArchitectureAssert.SinViolaciones(
            "D7 - Sin DateTime.Now en el núcleo",
            Codigo.Coincidencias(archivos, patron),
            "Inyecte TimeProvider en los handlers y pase la fecha a los métodos del dominio.");
    }

    [Fact, Trait("Hallazgo", "D3-A2")]
    public void Application_no_lanza_excepciones_de_dominio()
    {
        // Si Application lanza DomainException, la regla de negocio está fuera del dominio.
        var patron = Codigo.Patron(@"throw\s+new\s+DomainException\b");

        ArchitectureAssert.SinViolaciones(
            "D3-A2 - DomainException solo en Domain",
            Codigo.Coincidencias(RutasSolucion.ArchivosCs(Capas.Application), patron),
            "Mueva la validación al agregado o a un servicio de dominio y deje que sea el dominio quien la lance.");
    }
}
