using COFIDE.WFF.Architecture.Tests.Support;

namespace COFIDE.WFF.Architecture.Tests.Reglas;

/// <summary>
/// Regla de dependencias de la arquitectura hexagonal a nivel de proyecto (.csproj):
/// las dependencias apuntan hacia el núcleo (Domain) y nunca hacia los adaptadores.
/// </summary>
[Trait("Categoria", "Hexagonal")]
public class ReferenciasEntreProyectosTest
{

    /// <summary>Paquetes que representan tecnología de adaptadores (web, datos, HTTP, archivos, logging).</summary>
    private static readonly string[] PaquetesTecnologicos =
    {
        "Microsoft.AspNetCore", "Microsoft.EntityFrameworkCore", "Oracle.", "Microsoft.Extensions.Http",
        "System.Net.Http", "Serilog", "PdfSharp", "PDFsharp", "BouncyCastle", "Portable.BouncyCastle",
        "itext", "System.ServiceModel", "Dapper", "Microsoft.Data", "System.Data", "Swashbuckle",
        "Microsoft.Identity", "System.IdentityModel",
    };

   

    [Fact, Trait("Hallazgo", "Base")]
    public void Domain_no_referencia_paquetes_ni_frameworks()
    {
        var domain = ProyectoMsBuild.Cargar(Capas.Domain);

        ArchitectureAssert.SinViolaciones(
            "Base - Domain sin paquetes externos",
            domain.Paquetes.Select(p => $"PackageReference {p.Id}")
                .Concat(domain.Frameworks.Select(f => $"FrameworkReference {f}")),
            "El dominio debe expresarse solo con C# y la BCL.");
    }

   

    [Fact, Trait("Hallazgo", "H1")]
    public void Application_no_referencia_paquetes_de_tecnologia_de_adaptadores()
    {
        var application = ProyectoMsBuild.Cargar(Capas.Application);

        var violaciones = application.Paquetes
            .Where(p => PaquetesTecnologicos.Any(t => p.Id.StartsWith(t, StringComparison.OrdinalIgnoreCase)))
            .Select(p => p.Id)
            .Concat(application.Frameworks.Select(f => $"FrameworkReference {f}"));

        ArchitectureAssert.SinViolaciones(
            "H1 - Application sin paquetes tecnológicos",
            violaciones,
            "Reemplace IFormFile por ArchivoEntrada y elimine los paquetes web/HTTP de Application.");
    }

    [Fact, Trait("Hallazgo", "Base")]
    public void Infrastructure_no_referencia_Api_ni_Worker()
    {
        var infrastructure = ProyectoMsBuild.Cargar(Capas.Infrastructure);

        ArchitectureAssert.SinViolaciones(
            "Base - Infrastructure no referencia hosts",
            infrastructure.ReferenciasProyecto.Where(r => r == Capas.Api || r == Capas.Worker),
            "Los adaptadores no dependen de los hosts; la composición ocurre en Api/Worker.");
    }

    [Fact, Trait("Hallazgo", "Base")]
    public void Api_y_Worker_no_se_referencian_entre_si()
    {
        var violaciones = new List<string>();
        if (ProyectoMsBuild.Cargar(Capas.Api).ReferenciasProyecto.Contains(Capas.Worker))
            violaciones.Add($"{Capas.Api} -> {Capas.Worker}");
        if (ProyectoMsBuild.Existe(Capas.Worker)
            && ProyectoMsBuild.Cargar(Capas.Worker).ReferenciasProyecto.Contains(Capas.Api))
            violaciones.Add($"{Capas.Worker} -> {Capas.Api}");

        ArchitectureAssert.SinViolaciones(
            "Base - Hosts independientes",
            violaciones,
            "Api y Worker son dos hosts; lo que compartan debe vivir en Infrastructure o CrossCutting.");
    }

  

    [Fact, Trait("Categoria", "BuenasPracticas"), Trait("Hallazgo", "B9")]
    public void Versiones_de_paquetes_consistentes_y_sin_comodines()
    {
        var proyectos = RutasSolucion.ProyectosSrc().Select(ProyectoMsBuild.Cargar).ToList();
        var violaciones = new List<string>();

        // 1. Versiones flotantes
        violaciones.AddRange(proyectos.SelectMany(p => p.Paquetes
            .Where(pk => pk.Version.Contains('*'))
            .Select(pk => $"{p.Nombre}: {pk.Id} usa versión flotante '{pk.Version}'")));

        // 2. Mismo paquete con versiones distintas entre proyectos
        violaciones.AddRange(proyectos
            .SelectMany(p => p.Paquetes.Select(pk => (Proyecto: p.Nombre, pk.Id, pk.Version)))
            .GroupBy(x => x.Id, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Select(x => x.Version).Distinct().Count() > 1)
            .Select(g => $"{g.Key} con versiones distintas: " +
                         string.Join(", ", g.Select(x => $"{x.Proyecto}={x.Version}"))));

        // 3. Paquetes de Microsoft desalineados con el TargetFramework (p. ej. 10.x o 2.x en net8.0)
        foreach (var p in proyectos)
        {
            var major = MajorDeTarget(p.TargetFramework);
            if (major is null) continue;

            violaciones.AddRange(p.Paquetes
                .Where(pk => pk.Id.StartsWith("Microsoft.Extensions.", StringComparison.OrdinalIgnoreCase)
                          || pk.Id.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.OrdinalIgnoreCase)
                          || pk.Id.StartsWith("Microsoft.AspNetCore", StringComparison.OrdinalIgnoreCase))
                .Where(pk => MajorDeVersion(pk.Version) is int v && v != major)
                .Select(pk => $"{p.Nombre}: {pk.Id} {pk.Version} no corresponde a {p.TargetFramework}"));
        }

        ArchitectureAssert.SinViolaciones(
            "B9 - Versiones de paquetes",
            violaciones,
            "Centralice versiones (Directory.Packages.props), alinéelas con el TargetFramework y use FrameworkReference para ASP.NET Core.");
    }

    private static int? MajorDeTarget(string tfm)
    {
        // "net8.0" -> 8
        var digitos = new string(tfm.SkipWhile(c => !char.IsDigit(c)).TakeWhile(char.IsDigit).ToArray());
        return int.TryParse(digitos, out var v) ? v : null;
    }

    private static int? MajorDeVersion(string version)
    {
        var primero = version.Split('.')[0];
        return int.TryParse(primero, out var v) ? v : null;
    }
}
