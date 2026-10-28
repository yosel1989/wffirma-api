
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace COFIDE.WFF.Infrastructure.Adapters.Services.Security;

public class SolicitudIdProtector : ISolicitudIdProtector
{
    private readonly byte[] _clave;

    public SolicitudIdProtector(IConfiguration configuration)
    {
        var claveBase64 = configuration["SolicitudEncryption:Key"];

        if (string.IsNullOrWhiteSpace(claveBase64))
            throw new InvalidOperationException(
                "No se configuró la clave de cifrado de solicitudes.");

        _clave = Convert.FromBase64String(claveBase64);

        if (_clave.Length != 32)
            throw new InvalidOperationException(
                "La clave AES debe tener 32 bytes.");
    }

    public string Encriptar(long idSolicitud)
    {
        if (idSolicitud <= 0)
            throw new ArgumentOutOfRangeException(nameof(idSolicitud));

        byte[] datos = Encoding.UTF8.GetBytes(
            idSolicitud.ToString(CultureInfo.InvariantCulture));

        byte[] nonce = RandomNumberGenerator.GetBytes(12);
        byte[] cifrado = new byte[datos.Length];
        byte[] tag = new byte[16];

        using (var aes = new AesGcm(_clave, 16))
        {
            aes.Encrypt(nonce, datos, cifrado, tag);
        }

        // Token = nonce + tag + contenido cifrado
        byte[] resultado = new byte[nonce.Length + tag.Length + cifrado.Length];

        Buffer.BlockCopy(nonce, 0, resultado, 0, nonce.Length);
        Buffer.BlockCopy(tag, 0, resultado, nonce.Length, tag.Length);
        Buffer.BlockCopy(cifrado, 0, resultado, nonce.Length + tag.Length, cifrado.Length);

        return Convert.ToBase64String(resultado)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public long Desencriptar(string token)
    {
        if (string.IsNullOrWhiteSpace(token) || token.Length > 4096)
            throw new CryptographicException("El enlace no es válido.");

        try
        {
            string base64 = token.Replace('-', '+').Replace('_', '/');
            base64 = base64.PadRight(
                base64.Length + (4 - base64.Length % 4) % 4, '=');

            byte[] datos = Convert.FromBase64String(base64);

            if (datos.Length <= 28)
                throw new CryptographicException("El enlace no es válido.");

            byte[] nonce = datos[..12];
            byte[] tag = datos[12..28];
            byte[] cifrado = datos[28..];
            byte[] original = new byte[cifrado.Length];

            using (var aes = new AesGcm(_clave, 16))
            {
                aes.Decrypt(nonce, cifrado, tag, original);
            }

            string valor = Encoding.UTF8.GetString(original);

            if (!long.TryParse(
                    valor,
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out long idSolicitud) || idSolicitud <= 0)
            {
                throw new CryptographicException("El enlace no es válido.");
            }

            return idSolicitud;
        }
        catch (FormatException ex)
        {
            throw new CryptographicException("El enlace no es válido.", ex);
        }
    }
}
