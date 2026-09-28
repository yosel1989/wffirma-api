using System.Linq.Expressions;
using System.Reflection;
namespace COFIDE.WFF.Infrastructure.Extensions;

public static class QueryableExtensions
{

    public static IQueryable<T> OrderByDynamic<T>(this IQueryable<T> query, string sortBy, bool descending)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.PropertyOrField(parameter, sortBy);
        var lambda = Expression.Lambda(property, parameter);

        var methodName = descending ? "OrderByDescending" : "OrderBy";
        var method = typeof(Queryable).GetMethods()
            .First(m => m.Name == methodName && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(T), property.Type);

        return (IQueryable<T>)method.Invoke(null, new object[] { query, lambda })!;
    }

    public static IQueryable<T> OrderByMultiple<T>(
       this IQueryable<T> source,
       IEnumerable<(string PropertyName, bool Descending)> orderings)
    {
        if (orderings == null || !orderings.Any())
            return source;

        IOrderedQueryable<T>? orderedQuery = null;

        foreach (var (propertyPath, descending) in orderings)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            Expression propertyAccess = parameter;

            foreach (var propertyName in propertyPath.Split('.'))
            {
                var propertyInfo = propertyAccess.Type.GetProperty(propertyName,
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (propertyInfo == null)
                    throw new ArgumentException($"La propiedad '{propertyName}' no existe en '{propertyAccess.Type.Name}'.");

                propertyAccess = Expression.Property(propertyAccess, propertyInfo);
            }

            var lambda = Expression.Lambda(propertyAccess, parameter);

            var methodName = GetOrderingMethodName(orderedQuery == null, descending);
            var method = typeof(Queryable).GetMethods()
                .First(m => m.Name == methodName
                         && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), propertyAccess.Type);

            orderedQuery = (orderedQuery == null)
                ? (IOrderedQueryable<T>)method.Invoke(null, new object[] { source, lambda })!
                : (IOrderedQueryable<T>)method.Invoke(null, new object[] { orderedQuery, lambda })!;
        }

        return orderedQuery ?? source;
    }

    private static string GetOrderingMethodName(bool isFirst, bool descending)
    {
        if (isFirst)
            return descending ? "OrderByDescending" : "OrderBy";
        else
            return descending ? "ThenByDescending" : "ThenBy";
    }
}
