using System.Linq.Expressions;
using System.Reflection;

namespace COFIDE.WFF.Application.Tests.Support;

/// <summary>
/// Simula la carga de navegaciones que hace EF Core (D6: las entidades ya no exponen setters públicos).
/// Uso: NavegacionEf.Asignar(detalle, d => d.SolicitudDocumento, documento);
/// Solo para pruebas: en producción las navegaciones las asigna EF al consultar.
/// </summary>
internal static class NavegacionEf
{
    public static void Asignar<TEntidad, TValor>(TEntidad entidad, Expression<Func<TEntidad, TValor>> propiedad, TValor valor)
        where TEntidad : class
    {
        var miembro = (propiedad.Body as MemberExpression)?.Member as PropertyInfo
            ?? throw new ArgumentException("La expresión debe apuntar a una propiedad.", nameof(propiedad));

        var setter = miembro.GetSetMethod(nonPublic: true)
            ?? throw new InvalidOperationException($"{typeof(TEntidad).Name}.{miembro.Name} no tiene setter.");

        setter.Invoke(entidad, [valor]);
    }
}
