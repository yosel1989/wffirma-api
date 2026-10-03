using COFIDE.WFF.Domain.Common.Abstractions;
namespace COFIDE.WFF.Application.Common.Ports.Out;

/// <summary>
/// Publica los eventos post-commit (correos, historial, notificaciones en tiempo real) después de confirmar
/// la transacción. La implementación actual los publica de inmediato y reintenta en segundo plano si fallan;
/// en el futuro puede reemplazarse por una cola de mensajes sin cambiar a quienes la usan.
/// Nunca lanza excepciones: la transacción ya se confirmó y el usuario no debe recibir un error por esto.
/// </summary>
public interface IPublicadorEventosPostCommit
{
    Task PublicarAsync(IReadOnlyCollection<IDomainEvent> eventos, CancellationToken cancellationToken);
}
