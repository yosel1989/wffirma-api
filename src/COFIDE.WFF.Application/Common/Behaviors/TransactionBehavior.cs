using COFIDE.WFF.Application.Common.Events;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Domain.Common.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;
namespace COFIDE.WFF.Application.Common.Behaviors;

public sealed class TransactionBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDomainEventDispatcher _dispatcher;
    private readonly IPublicadorEventosPostCommit _publicador;
    private readonly IRegistroArchivosTransaccion _archivos;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(
        IUnitOfWork unitOfWork,
        IDomainEventDispatcher dispatcher,
        IPublicadorEventosPostCommit publicador,
        IRegistroArchivosTransaccion archivos,
        ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _unitOfWork = unitOfWork;
        _dispatcher = dispatcher;
        _publicador = publicador;
        _archivos = archivos;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        if (!requestName.EndsWith("Command"))
            return await next(cancellationToken);

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var response = await next(cancellationToken);

            var events = _unitOfWork.GetDomainEvents();

            await _dispatcher.DispatchPreCommitAsync(
                events,
                cancellationToken);

            await _unitOfWork.CommitTransactionAsync(
                cancellationToken);

            _archivos.ConfirmarArchivos();

            // Publicación post-commit: se intenta de inmediato (el usuario recibe los correos como antes).
            // Si falla, el publicador reintenta en segundo plano; nunca devuelve error al usuario.
            await _publicador.PublicarAsync(
                events.Where(e => e is IPostCommitDomainEvent).ToList(),
                CancellationToken.None);

            return response;
        }
        catch (Exception ex) when (LogTransactionError(ex, requestName))
        {
            await _unitOfWork.RollbackTransactionAsync();

            // Compensación: deshace los archivos escritos durante la transacción revertida.
            await _archivos.DescartarArchivosAsync(CancellationToken.None);
            throw;
        }
    }

    private bool LogTransactionError(Exception ex, string requestName)
    {
        _logger.LogError(ex,
            "La transacción falló para {RequestName}",
            requestName);

        return true;
    }
}