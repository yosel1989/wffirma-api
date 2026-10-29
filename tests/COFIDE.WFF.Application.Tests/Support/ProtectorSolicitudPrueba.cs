using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using Moq;

namespace COFIDE.WFF.Application.Tests.Support;

/// <summary>
/// Protector del identificador de la solicitud para las pruebas: devuelve "token-{id}" en lugar del valor cifrado,
/// para poder verificar el enlace del correo sin depender de la clave de cifrado.
/// </summary>
internal static class ProtectorSolicitudPrueba
{
    public const string Prefijo = "token-";

    public static ISolicitudIdProtector Crear()
    {
        var protector = new Mock<ISolicitudIdProtector>();
        protector.Setup(p => p.Encriptar(It.IsAny<long>()))
            .Returns<long>(id => $"{Prefijo}{id}");
        protector.Setup(p => p.Desencriptar(It.Is<string>(t => t.StartsWith(Prefijo))))
            .Returns<string>(t => long.Parse(t[Prefijo.Length..]));
        return protector.Object;
    }
}
