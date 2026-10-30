using System.Linq.Expressions;
using System.Reflection;


namespace COFIDE.WFF.Infrastructure.Tests.Helpers
{
    public static class TestExtensions
    {
        public static T Con<T, TValue>(this T entidad, Expression<Func<T, TValue>> propiedad, TValue valor)
        {
            var nombre = ((MemberExpression)propiedad.Body).Member.Name;
            typeof(T).GetProperty(nombre, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
                .SetValue(entidad, valor);
            return entidad;
        }
    }
}
