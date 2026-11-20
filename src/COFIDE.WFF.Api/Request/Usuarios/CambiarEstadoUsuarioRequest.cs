namespace COFIDE.WFF.Api.Request.Usuarios;

/// <summary>Estado de activacion solicitado para un usuario.</summary>
public sealed record CambiarEstadoUsuarioRequest
{
    public required bool Estado { get; init; }
}
