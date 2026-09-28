namespace COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
public interface ICorreoSenderPort
{
    Task EnviarCorreoAsync(
        string asunto,
        string cuerpo,
        string destinatario,
        string remitente,
        CancellationToken cancellationToken = default);
}
