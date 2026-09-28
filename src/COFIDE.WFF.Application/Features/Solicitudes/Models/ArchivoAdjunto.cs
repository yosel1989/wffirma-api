namespace COFIDE.WFF.Application.Features.Solicitudes.Models;

public sealed class ArchivoAdjunto
{
    public string Nombre { get; init; } = default!;
    public string ContentType { get; init; } = default!;
    public byte[] Contenido { get; init; } = default!;
}
