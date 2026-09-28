namespace COFIDE.WFF.Domain.Common.ValueObjects;

/// <summary>
/// Base abstracto para todos los Value Objects en DDD,
/// asegurando inmutabilidad y comparaciones por valor.
/// </summary>
public abstract class ValueObject
{
    /// <summary>
    /// Obtiene los componentes que identifican la igualdad del Value Object.
    /// </summary>
    /// <returns>IEnumerable con los valores que definen la igualdad.</returns>
    protected abstract IEnumerable<object> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != GetType())
            return false;

        var other = (ValueObject)obj;

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        // Usa XOR para combinar hash codes de cada componente de igualdad
        return GetEqualityComponents()
            .Aggregate(0, (hash, obj) => HashCode.Combine(hash, obj != null ? obj.GetHashCode() : 0));
    }
}