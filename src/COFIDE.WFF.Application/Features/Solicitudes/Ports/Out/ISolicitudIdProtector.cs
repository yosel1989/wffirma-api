namespace COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;

public interface ISolicitudIdProtector
{
    string Encriptar(long idSolicitud);
    long Desencriptar(string token);
}