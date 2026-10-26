using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using Microsoft.AspNetCore.DataProtection;

namespace COFIDE.WFF.Infrastructure.Adapters.Services.Security;

public class SolicitudIdProtector : ISolicitudIdProtector
{
    private readonly ITimeLimitedDataProtector _protector;

    public SolicitudIdProtector(
        IDataProtectionProvider provider)
    {
        _protector = provider
            .CreateProtector("WFF.Solicitudes.EmailLinks.v1")
            .ToTimeLimitedDataProtector();
    }

    public string Encriptar(long idSolicitud)
    {
        return _protector.Protect(
            idSolicitud.ToString(),
            TimeSpan.FromDays(30));
    }

    public long Desencriptar(string token)
    {
        var valor = _protector.Unprotect(token);

        if (!long.TryParse(valor, out var idSolicitud) ||
            idSolicitud <= 0)
        {
            throw new InvalidOperationException(
                "El enlace de la solicitud no es válido.");
        }

        return idSolicitud;
    }
}