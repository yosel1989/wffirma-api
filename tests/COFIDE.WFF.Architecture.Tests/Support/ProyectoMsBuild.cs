using System.Xml.Linq;

namespace COFIDE.WFF.Architecture.Tests.Support;

/// <summary>Lectura simple de un .csproj (referencias de proyecto, paquetes y frameworks).</summary>
public sealed class ProyectoMsBuild
{
    private ProyectoMsBuild(string nombre, XDocument xml)
    {
        Nombre = nombre;

        ReferenciasProyecto = Elementos(xml, "ProjectReference")
            .Select(e => (string?)e.Attribute("Include"))
            .OfType<string>()
            .Select(r => Path.GetFileNameWithoutExtension(r.Replace('\\', '/')))
            .ToList();

        Paquetes = Elementos(xml, "PackageReference")
            .Select(e => new Paquete(
                (string?)e.Attribute("Include") ?? "",
                (string?)e.Attribute("Version") ?? Elementos(e, "Version").FirstOrDefault()?.Value ?? ""))
            .Where(p => p.Id.Length > 0)
            .ToList();

        Frameworks = Elementos(xml, "FrameworkReference")
            .Select(e => (string?)e.Attribute("Include"))
            .OfType<string>()
            .ToList();

        TargetFramework = Elementos(xml, "TargetFramework").FirstOrDefault()?.Value ?? "";
        Sdk = (string?)xml.Root?.Attribute("Sdk") ?? "";
    }

    public string Nombre { get; }
    public string Sdk { get; }
    public string TargetFramework { get; }
    public IReadOnlyList<string> ReferenciasProyecto { get; }
    public IReadOnlyList<Paquete> Paquetes { get; }
    public IReadOnlyList<string> Frameworks { get; }

    public static bool Existe(string proyecto) => File.Exists(RutasSolucion.Csproj(proyecto));

    public static ProyectoMsBuild Cargar(string proyecto) =>
        new(proyecto, XDocument.Load(RutasSolucion.Csproj(proyecto)));

    private static IEnumerable<XElement> Elementos(XContainer contenedor, string nombreLocal) =>
        contenedor.Descendants().Where(e => e.Name.LocalName == nombreLocal);
}

public sealed record Paquete(string Id, string Version);
