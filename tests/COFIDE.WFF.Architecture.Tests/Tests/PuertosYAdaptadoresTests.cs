using COFIDE.WFF.Architecture.Tests.Support;

namespace COFIDE.WFF.Architecture.Tests.Reglas;

/// <summary>
/// Puertos (interfaces en Application) y adaptadores (implementaciones fuera del núcleo).
/// </summary>
[Trait("Categoria", "Hexagonal")]
public class PuertosYAdaptadoresTests
{
    private static IReadOnlyList<Type> TiposApplication => Capas.Tipos(Capas.ApplicationAssembly);

    private static IReadOnlyList<Type> TiposAdaptadores =>
        Capas.Tipos(Capas.InfrastructureAssembly).Concat(Capas.Tipos(Capas.ApiAssembly)).ToList();

    private static bool EsNamespaceDePuertoSalida(Type t) => t.Namespace?.Contains(".Ports.Out") == true;

    [Fact, Trait("Hallazgo", "Base")]
    public void Namespaces_de_puertos_solo_contienen_interfaces()
    {
        var violaciones = TiposApplication
            .Where(t => t.Namespace?.Contains(".Ports.") == true || t.Namespace?.EndsWith(".Ports") == true)
            .Where(t => !t.IsInterface)
            .Select(t => t.NombreCompleto());

        ArchitectureAssert.SinViolaciones(
            "Base - Puertos son interfaces",
            violaciones,
            "Un puerto es un contrato. Las implementaciones van en los adaptadores y los DTO fuera de la carpeta Ports.");
    }

    [Fact, Trait("Hallazgo", "Base")]
    public void Application_no_implementa_sus_propios_puertos_de_salida()
    {
        var violaciones = TiposApplication
            .Where(t => t.EsClaseConcreta())
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.Assembly == Capas.ApplicationAssembly && EsNamespaceDePuertoSalida(i))
                .Select(i => $"{t.NombreCompleto()} implementa {i.NombreCompleto()}"));

        ArchitectureAssert.SinViolaciones(
            "Base - Puertos de salida implementados fuera del núcleo",
            violaciones,
            "Los puertos de salida (Ports.Out) deben implementarse en Infrastructure (o en el host).");
    }

    [Fact, Trait("Hallazgo", "Base")]
    public void Cada_puerto_de_salida_tiene_un_adaptador()
    {
        var implementadas = TiposAdaptadores
            .Where(t => t.EsClaseConcreta())
            .SelectMany(t => t.GetInterfaces())
            .Select(i => i.IsGenericType ? i.GetGenericTypeDefinition() : i)
            .ToHashSet();

        var violaciones = TiposApplication
            .Where(t => t.IsInterface && EsNamespaceDePuertoSalida(t))
            .Where(t => !implementadas.Contains(t))
            .Select(t => t.NombreCompleto());

        ArchitectureAssert.SinViolaciones(
            "Base - Puertos de salida con adaptador",
            violaciones,
            "Puerto sin implementación en Infrastructure/Api: elimínelo si no se usa, o verifique si solo lo implementa el Worker.");
    }

    [Fact, Trait("Hallazgo", "H4")]
    public void Interfaces_implementadas_por_adaptadores_estan_en_un_namespace_Ports_Out()
    {
        var violaciones = TiposAdaptadores
            .Where(t => t.EsClaseConcreta())
            .SelectMany(t => t.GetInterfaces())
            .Select(i => i.IsGenericType ? i.GetGenericTypeDefinition() : i)
            .Where(i => i.Assembly == Capas.ApplicationAssembly)
            .Where(i => !EsNamespaceDePuertoSalida(i))
            .Select(i => i.NombreCompleto())
            .Distinct();

        ArchitectureAssert.SinViolaciones(
            "H4 - Puertos en convención única",
            violaciones,
            "Toda interfaz de Application implementada por un adaptador es un puerto: ubíquela en Common/Ports/Out o Features/<Feature>/Ports/Out.");
    }

    [Fact, Trait("Hallazgo", "H4")]
    public void No_existe_el_namespace_raiz_Application_Ports()
    {
        var violaciones = TiposApplication
            .Where(t => t.Namespace?.StartsWith($"{Capas.Application}.Ports", StringComparison.Ordinal) == true)
            .Select(t => t.NombreCompleto());

        ArchitectureAssert.SinViolaciones(
            "H4 - Sin carpeta Application/Ports",
            violaciones,
            "Mueva los puertos transversales a Application/Common/Ports/Out.");
    }

    [Fact, Trait("Categoria", "CQRS"), Trait("Hallazgo", "A1")]
    public void No_hay_puertos_de_entrada_redundantes_con_MediatR()
    {
        var violaciones = TiposApplication
            .Where(t => t.Namespace?.Contains(".Ports.In") == true || t.Name.EndsWith("UseCase", StringComparison.Ordinal))
            .Select(t => t.NombreCompleto());

        ArchitectureAssert.SinViolaciones(
            "A1 - Sin capa UseCase sobre MediatR",
            violaciones,
            "Con CQRS/MediatR el Handler es el caso de uso: mueva la lógica al Handler y elimine UseCase y Ports.In.");
    }

    [Fact, Trait("Hallazgo", "Base")]
    public void Repositorios_de_Infrastructure_implementan_un_puerto()
    {
        var violaciones = Capas.Tipos(Capas.InfrastructureAssembly)
            .Where(t => t.EsClaseConcreta() && t.Name.EndsWith("Repository", StringComparison.Ordinal))
            .Where(t => !t.GetInterfaces().Any(i => i.Assembly == Capas.ApplicationAssembly))
            .Select(t => t.NombreCompleto());

        ArchitectureAssert.SinViolaciones(
            "Base - Repositorios implementan puertos",
            violaciones,
            "Cada repositorio concreto debe implementar un puerto definido en Application.");
    }
}
