using MediatR;
namespace COFIDE.WFF.Application.Common.Abstractions;

/// <summary>
/// Contrato genérico para los handlers de comandos y consultas.
/// Proporciona una firma común para facilitar inyección, testing o decoración.
/// </summary>
/// <typeparam name="TRequest">Tipo del mensaje (comando o consulta).</typeparam>
/// <typeparam name="TResponse">Tipo de respuesta esperada.</typeparam>
public interface IHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
{ }
