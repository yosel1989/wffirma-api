using System.Reflection;
using System.Runtime.CompilerServices;

namespace COFIDE.WFF.Architecture.Tests.Support;

public static class ReflexionExtensions
{
    public static IEnumerable<Type> TiposCargables(this Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.OfType<Type>();
        }
    }

    public static bool EsGeneradoPorCompilador(this Type tipo) =>
        tipo.IsDefined(typeof(CompilerGeneratedAttribute), false)
        || tipo.Name.Contains('<')
        || (tipo.DeclaringType?.EsGeneradoPorCompilador() ?? false);

    public static bool EsMiembroGeneradoPorCompilador(this MemberInfo miembro) =>
        miembro.IsDefined(typeof(CompilerGeneratedAttribute), false) || miembro.Name.Contains('<');

    /// <summary>Nombre legible: Namespace.Tipo (sin aridad genérica ni tipos anidados).</summary>
    public static string NombreCompleto(this Type tipo)
    {
        var nombre = tipo.FullName ?? $"{tipo.Namespace}.{tipo.Name}";
        return NormalizarNombreTipo(nombre);
    }

    /// <summary>
    /// Normaliza nombres de NetArchTest (Cecil) y de reflexión:
    /// "Ns.Clase/&lt;Handle&gt;d__5" o "Ns.Clase+Anidada" → "Ns.Clase"; "Ns.Tipo`1" → "Ns.Tipo".
    /// </summary>
    public static string NormalizarNombreTipo(string nombre)
    {
        var n = nombre.Split('/', '+')[0];
        var tick = n.IndexOf('`');
        return tick >= 0 ? n[..tick] : n;
    }

    public static bool TieneSetterPublicoMutable(this PropertyInfo propiedad)
    {
        var setter = propiedad.SetMethod;
        return setter is not null && setter.IsPublic && !propiedad.EsInitOnly();
    }

    public static bool EsInitOnly(this PropertyInfo propiedad) =>
        propiedad.SetMethod is { } setter
        && setter.ReturnParameter.GetRequiredCustomModifiers()
            .Any(m => m.FullName == "System.Runtime.CompilerServices.IsExternalInit");

    /// <summary>El tipo y todos los tipos que contiene (argumentos genéricos, elementos de arreglos).</summary>
    public static IEnumerable<Type> TiposContenidos(this Type tipo)
    {
        yield return tipo;

        if (tipo.HasElementType && tipo.GetElementType() is { } elemento)
            foreach (var t in elemento.TiposContenidos())
                yield return t;

        if (tipo.IsGenericType)
            foreach (var argumento in tipo.GetGenericArguments())
                foreach (var t in argumento.TiposContenidos())
                    yield return t;
    }

    public static bool ImplementaInterfaz(this Type tipo, string nombreCompletoDefinicion) =>
        tipo.GetInterfaces().Any(i =>
            (i.IsGenericType ? i.GetGenericTypeDefinition().FullName : i.FullName) == nombreCompletoDefinicion);

    /// <summary>Commands y Queries de MediatR.</summary>
    public static bool EsRequestMediatR(this Type tipo) =>
        tipo.ImplementaInterfaz("MediatR.IBaseRequest")
        || tipo.ImplementaInterfaz("MediatR.IRequest")
        || tipo.ImplementaInterfaz("MediatR.IRequest`1");

    public static bool EsHandlerMediatR(this Type tipo) =>
        tipo.ImplementaInterfaz("MediatR.IRequestHandler`1")
        || tipo.ImplementaInterfaz("MediatR.IRequestHandler`2");

    /// <summary>Clase concreta instanciable (excluye abstractas, interfaces y definiciones genéricas abiertas).</summary>
    public static bool EsClaseConcreta(this Type tipo) =>
        tipo.IsClass && !tipo.IsAbstract && !tipo.IsGenericTypeDefinition;

    public static bool HeredaDe(this Type tipo, string nombreSimpleBase)
    {
        for (var actual = tipo.BaseType; actual is not null; actual = actual.BaseType)
            if (NormalizarNombreTipo(actual.Name) == nombreSimpleBase)
                return true;
        return false;
    }

    public static bool EsTipoAwaitable(this Type tipo)
    {
        if (tipo == typeof(Task) || tipo == typeof(ValueTask))
            return true;
        if (!tipo.IsGenericType)
            return false;
        var def = tipo.GetGenericTypeDefinition();
        return def == typeof(Task<>) || def == typeof(ValueTask<>) || def == typeof(IAsyncEnumerable<>);
    }

    public static IEnumerable<MethodInfo> MetodosDeclarados(this Type tipo) =>
        tipo.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                        | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialName && !m.EsMiembroGeneradoPorCompilador());
}
