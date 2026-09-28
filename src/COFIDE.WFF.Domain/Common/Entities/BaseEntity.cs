using COFIDE.WFF.Domain.Common.Abstractions;

namespace COFIDE.WFF.Domain.Common.Entities;

/// <summary>
/// Implementación base de <see cref="IEntity{TKey}"/>, que provee la propiedad <c>Id</c>.
/// </summary>
public abstract class BaseEntity<TKey> : IEntity<TKey> where TKey : struct
{
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();


    /// <inheritdoc/>
    public virtual TKey Id { get; protected set; }

    private readonly List<IDomainEvent> _domainEvents = new();
    protected BaseEntity()
    {
        // si TKey es Guid, se puede  inicializar aquí:
        if (typeof(TKey) == typeof(Guid))
        {
            Id = (TKey)(object)Guid.NewGuid();
        }
    }

    // Opcional: igualdad basada en Id
    public override bool Equals(object? obj)
    {
        if (obj is not BaseEntity<TKey> other) return false;
        if (ReferenceEquals(this, other)) return true;
        if (Id.Equals(other.Id) is true) return true;
        return false;

    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }


    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public static HashSet<string> Normalizar(List<string>? valores)
    {
        return (valores ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim().ToUpperInvariant())
            .ToHashSet();
    }

}