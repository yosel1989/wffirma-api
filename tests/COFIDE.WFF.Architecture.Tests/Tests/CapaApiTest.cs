using System.Reflection;
using COFIDE.WFF.Architecture.Tests.Support;
using Microsoft.AspNetCore.Mvc;

namespace COFIDE.WFF.Architecture.Tests.Reglas;

/// <summary>Reglas del adaptador de entrada HTTP (controladores y contratos).</summary>
[Trait("Categoria", "Hexagonal")]
public class CapaApiTest
{
    private static IEnumerable<Type> Controladores => Capas.Tipos(Capas.ApiAssembly)
        .Where(t => t.EsClaseConcreta() && typeof(ControllerBase).IsAssignableFrom(t));

    private static IEnumerable<(Type Controlador, MethodInfo Accion)> Acciones => Controladores
        .SelectMany(c => c.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialName && !m.IsDefined(typeof(NonActionAttribute), true))
            .Select(m => (Controlador: c, Accion: m)));

    private static string Nombre((Type Controlador, MethodInfo Accion) a) =>
        $"{a.Controlador.Name}.{a.Accion.Name}";

    [Fact, Trait("Hallazgo", "H6")]
    public void Acciones_no_reciben_Commands_ni_Queries()
    {
        var violaciones = Acciones
            .SelectMany(a => a.Accion.GetParameters()
                .Where(p => p.ParameterType.EsRequestMediatR())
                .Select(p => $"{Nombre(a)}({p.ParameterType.Name})"));

        ArchitectureAssert.SinViolaciones(
            "H6 - Controladores reciben Requests",
            violaciones,
            "Reciba un Request de la Api y conviértalo con request.ToCommand()/ToQuery() (Api/Mappings).");
    }

    [Fact, Trait("Hallazgo", "H6")]
    public void Acciones_no_reciben_modelos_definidos_en_Application()
    {
        var violaciones = Acciones
            .SelectMany(a => a.Accion.GetParameters()
                .Where(p => p.ParameterType.TiposContenidos().Any(t => t.Assembly == Capas.ApplicationAssembly))
                .Where(p => !p.ParameterType.EsRequestMediatR()) // ya cubierto por la regla anterior
                .Select(p => $"{Nombre(a)}({p.ParameterType.Name})"));

        ArchitectureAssert.SinViolaciones(
            "H6 - Contrato HTTP propio de la Api",
            violaciones,
            "El contrato HTTP (Request) debe definirse en la Api para no acoplarlo al modelo de Application.");
    }

    [Fact, Trait("Categoria", "BuenasPracticas"), Trait("Hallazgo", "B2")]
    public void Acciones_asincronas_reciben_CancellationToken()
    {
        var violaciones = Acciones
            .Where(a => a.Accion.ReturnType.EsTipoAwaitable())
            .Where(a => !a.Accion.GetParameters().Any(p => p.ParameterType == typeof(CancellationToken)))
            .Select(Nombre);

        ArchitectureAssert.SinViolaciones(
            "B2 - CancellationToken en acciones",
            violaciones,
            "Agregue 'CancellationToken cancellationToken' y páselo a _mediator.Send(command, cancellationToken).");
    }

    [Fact, Trait("Hallazgo", "Base")]
    public void Controladores_no_inyectan_repositorios_ni_casos_de_uso()
    {
        var violaciones = Controladores
            .SelectMany(c => c.GetConstructors()
                .SelectMany(k => k.GetParameters())
                .Where(p => p.ParameterType.Assembly == Capas.InfrastructureAssembly
                            || p.ParameterType.Name.EndsWith("Repository", StringComparison.Ordinal)
                            || p.ParameterType.Name.EndsWith("UseCase", StringComparison.Ordinal)
                            || p.ParameterType.Name.EndsWith("DbContext", StringComparison.Ordinal))
                .Select(p => $"{c.Name} recibe {p.ParameterType.Name}"));

        ArchitectureAssert.SinViolaciones(
            "Base - Controladores delgados",
            violaciones,
            "El controlador solo traduce HTTP ↔ Command/Query y delega en MediatR.");
    }

    [Fact, Trait("Categoria", "Nomenclatura"), Trait("Hallazgo", "N6")]
    public void Requests_de_la_Api_en_un_unico_namespace_raiz()
    {
        var requests = Capas.Tipos(Capas.ApiAssembly)
            .Where(t => t.Name.EndsWith("Request", StringComparison.Ordinal) && t.Namespace is not null)
            .ToList();

        // Primer segmento después de "COFIDE.WFF.Api." (p. ej. "Request", "Requests", "Contracts")
        var raices = requests
            .GroupBy(t => t.Namespace!.Length > Capas.Api.Length + 1
                ? t.Namespace![(Capas.Api.Length + 1)..].Split('.')[0]
                : "")
            .ToList();

        var violaciones = raices.Count <= 1
            ? Enumerable.Empty<string>()
            : raices.SelectMany(g => g.Select(t => $"{g.Key}: {t.NombreCompleto()}"));

        ArchitectureAssert.SinViolaciones(
            "N6 - Requests en una sola carpeta",
            violaciones,
            "Ubique todos los Requests bajo una misma raíz (p. ej. Api/Contracts/Requests/<Feature>).");
    }
}
