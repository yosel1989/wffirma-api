using MediatR;
namespace COFIDE.WFF.Application.Common.Abstractions;

/// <summary>
/// Contrato base para todos los comandos (acciones que modifican el estado del sistema).
/// </summary>
/// <typeparam name="TResponse">Tipo de respuesta que devuelve el comando (puede ser void, bool, ID, DTO, etc.).</typeparam>
public interface ICommand<out TResponse> : IRequest<TResponse> { }
public interface ICommand : IRequest { }
