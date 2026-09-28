using MediatR;
namespace COFIDE.WFF.Application.Common.Abstractions;

/// <summary>
/// Contrato base para todas las consultas del sistema (lecturas puras, sin efectos secundarios).
/// </summary>
/// <typeparam name="TResponse">Tipo de dato que devuelve la consulta.</typeparam>
public interface IQuery<out TResponse> : IRequest<TResponse>
{ }
