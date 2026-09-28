using AutoMapper;
using COFIDE.WFF.Application.Common.Abstractions;
namespace COFIDE.WFF.Application.Common.Handlers;


/// <summary>
/// Handler base para consultas simples (queries) en el patrón CQRS.
/// Útil para reutilizar la lógica de manejo y estructurar uniformemente los handlers de consultas.
/// </summary>
/// <typeparam name="TQuery">Tipo de la consulta (input).</typeparam>
/// <typeparam name="TResponse">Tipo del resultado que retorna la consulta.</typeparam>
public abstract class BaseQueryHandler<TQuery, TResponse> : IHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    /// <summary>
    /// Instancia de AutoMapper para transformar entidades a DTOs.
    /// </summary>
    protected readonly IMapper _mapper;

    /// <summary>
    /// Nombre opcional de la entidad que representa esta consulta, útil para logs o mensajes dinámicos.
    /// </summary>
    protected readonly string? _entityDisplayName;

    /// <summary>
    /// Inicializa una nueva instancia del <see cref="BaseQueryHandler{TQuery, TResponse}"/>.
    /// </summary>
    /// <param name="mapper">Instancia de AutoMapper.</param>
    /// <param name="entityDisplayName">Nombre amigable de la entidad (opcional).</param>
    protected BaseQueryHandler(IMapper mapper, string? entityDisplayName = null)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _entityDisplayName = entityDisplayName;
    }

    /// <summary>
    /// Ejecuta el manejo de la consulta.
    /// </summary>
    /// <param name="request">Instancia de la consulta.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Respuesta del tipo esperado.</returns>
    public abstract Task<TResponse> Handle(TQuery request, CancellationToken cancellationToken);
}
