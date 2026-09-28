using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using System.Security.Cryptography;
namespace COFIDE.WFF.Infrastructure.Adapters.Services.Security;

public class TokenSeguroGenerator : ITokenSeguroGenerator
{
    public string Generar()
    {
        return Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(64))
            .Replace("/", "_")
            .Replace("+", "-")
            .Replace("=", "");
    }
}
