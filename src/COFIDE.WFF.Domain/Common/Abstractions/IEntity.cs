namespace COFIDE.WFF.Domain.Common.Abstractions;

/// <summary>
/// Define el contrato base para toda entidad del dominio.
/// Una entidad se caracteriza por tener una identidad única que persiste en el tiempo,
/// independientemente de los cambios en sus propiedades.
/// </summary>
/// <typeparam name="TKey">
/// Tipo de dato que representa la clave primaria o identificador único de la entidad.
/// Suele ser <c>Guid</c>, <c>int</c> o cualquier tipo valor comparable.
/// </typeparam>
public interface IEntity<out TKey> where TKey : struct
{
    TKey Id { get; }
}