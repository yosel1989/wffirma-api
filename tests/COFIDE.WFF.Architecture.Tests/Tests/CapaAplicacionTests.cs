using System.Reflection;
using COFIDE.WFF.Architecture.Tests.Support;

namespace COFIDE.WFF.Architecture.Tests.Reglas;

/// <summary>Reglas de la capa Application con CQRS (MediatR).</summary>
[Trait("Categoria", "CQRS")]
public class CapaAplicacionTests
{
    private static IReadOnlyList<Type> TiposApplication => Capas.Tipos(Capas.ApplicationAssembly);

    private static IEnumerable<Type> CommandsYQueries => TiposApplication
        .Where(t => t.EsClaseConcreta() && t.EsRequestMediatR());

    private static IEnumerable<Type> Handlers => TiposApplication
        .Where(t => t.EsClaseConcreta() && t.EsHandlerMediatR());

    [Fact, Trait("Categoria", "Hexagonal"), Trait("Hallazgo", "H1")]
    public void Commands_y_Queries_no_contienen_tipos_web()
    {
        var violaciones = CommandsYQueries
            .SelectMany(t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.PropertyType.TiposContenidos()
                    .Any(x => x.Namespace?.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal) == true))
                .Select(p => $"{t.NombreCompleto()}.{p.Name} : {p.PropertyType.Name}"));

        ArchitectureAssert.SinViolaciones(
            "H1 - Commands sin IFormFile",
            violaciones,
            "Convierta IFormFile a ArchivoEntrada en la Api (Request → Command) antes de enviar a MediatR.");
    }

    [Fact, Trait("Hallazgo", "B7")]
    public void Commands_y_Queries_terminan_en_Command_o_Query()
    {
        var violaciones = CommandsYQueries
            .Where(t => !t.Name.EndsWith("Command", StringComparison.Ordinal)
                        && !t.Name.EndsWith("Query", StringComparison.Ordinal))
            .Select(t => t.NombreCompleto());

        ArchitectureAssert.SinViolaciones(
            "B7 - Nombres de Commands y Queries",
            violaciones,
            "TransactionBehavior decide por el sufijo del nombre: mantenga la convención o use una interfaz marcadora.");
    }

    [Fact, Trait("Hallazgo", "Base")]
    public void Queries_no_implementan_la_interfaz_de_Command()
    {
        var violaciones = CommandsYQueries
            .Where(t => t.Name.EndsWith("Query", StringComparison.Ordinal))
            .Where(t => t.GetInterfaces().Any(i => i.Name.StartsWith("ICommand", StringComparison.Ordinal)))
            .Select(t => t.NombreCompleto());

        ArchitectureAssert.SinViolaciones(
            "Base - Queries no son Commands",
            violaciones,
            "Una Query solo lee: no debe entrar al pipeline transaccional de los Commands.");
    }

    [Fact, Trait("Hallazgo", "Base")]
    public void Handlers_no_dependen_de_otros_handlers()
    {
        var violaciones = Handlers
            .SelectMany(h => h.GetConstructors()
                .SelectMany(c => c.GetParameters())
                .Where(p => p.ParameterType.EsHandlerMediatR())
                .Select(p => $"{h.NombreCompleto()} recibe {p.ParameterType.Name}"));

        ArchitectureAssert.SinViolaciones(
            "Base - Handlers independientes",
            violaciones,
            "Extraiga la lógica compartida a un servicio de dominio o a un colaborador; no encadene handlers.");
    }

    [Fact, Trait("Hallazgo", "A1")]
    public void Handlers_no_delegan_en_UseCases()
    {
        var violaciones = Handlers
            .SelectMany(h => h.GetConstructors()
                .SelectMany(c => c.GetParameters())
                .Where(p => p.ParameterType.Name.EndsWith("UseCase", StringComparison.Ordinal))
                .Select(p => $"{h.NombreCompleto()} recibe {p.ParameterType.Name}"));

        ArchitectureAssert.SinViolaciones(
            "A1 - Handlers sin UseCase intermedio",
            violaciones,
            "El Handler es el caso de uso: mueva la lógica del UseCase al Handler.");
    }

    [Fact, Trait("Categoria", "Hexagonal"), Trait("Hallazgo", "H6")]
    public void Application_no_define_Requests_HTTP()
    {
        var violaciones = TiposApplication
            .Where(t => !t.IsInterface)
            .Where(t => t.Name.EndsWith("Request", StringComparison.Ordinal)
                        || t.Name.EndsWith("RequestDto", StringComparison.Ordinal))
            .Select(t => t.NombreCompleto());

        ArchitectureAssert.SinViolaciones(
            "H6 - Requests fuera de Application",
            violaciones,
            "Los Requests son el contrato HTTP y viven en la Api (Contracts/Requests). En Application use Commands/Queries y tipos *Input.");
    }

    [Fact, Trait("Hallazgo", "Base")]
    public void Handlers_no_dependen_de_implementaciones_concretas_de_infraestructura()
    {
        var violaciones = Handlers
            .SelectMany(h => h.GetConstructors()
                .SelectMany(c => c.GetParameters())
                .Where(p => p.ParameterType.IsClass
                            && (p.ParameterType.Name.EndsWith("Repository", StringComparison.Ordinal)
                                || p.ParameterType.Name.EndsWith("Adapter", StringComparison.Ordinal)
                                || p.ParameterType.Name.EndsWith("Client", StringComparison.Ordinal)
                                || p.ParameterType.Name.EndsWith("DbContext", StringComparison.Ordinal)))
                .Select(p => $"{h.NombreCompleto()} recibe {p.ParameterType.Name}"));

        ArchitectureAssert.SinViolaciones(
            "Base - Handlers dependen de puertos",
            violaciones,
            "Inyecte la interfaz del puerto, no la clase concreta.");
    }
}
