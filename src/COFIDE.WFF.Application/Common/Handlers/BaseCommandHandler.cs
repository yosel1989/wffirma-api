using AutoMapper;
using COFIDE.WFF.Application.Common.Abstractions;
namespace COFIDE.WFF.Application.Common.Handlers;

public abstract class BaseCommandHandler<TCommand, TResponse> : IHandler<TCommand, TResponse>
                                                                  where TCommand : ICommand<TResponse>
{
    /// <summary>
    /// Instancia de AutoMapper para transformar entidades a DTOs.
    /// </summary>
    protected readonly IMapper _mapper;

    /// <summary>
    /// Nombre opcional de la entidad que representa esta consulta, útil para logs o mensajes dinámicos.
    /// </summary>
    protected readonly string? _entityDisplayName;

    public abstract Task<TResponse> Handle(TCommand request, CancellationToken cancellationToken);

    /// <summary>
    /// Inicializa una nueva instancia del <see cref="BaseCommandHandler{TQuery, TResponse}"/>.
    /// </summary>
    /// <param name="mapper">Instancia de AutoMapper.</param>
    /// <param name="entityDisplayName">Nombre amigable de la entidad (opcional).</param>
    protected BaseCommandHandler(IMapper mapper, string? entityDisplayName = null)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _entityDisplayName = entityDisplayName;
    }
}
