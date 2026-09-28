namespace COFIDE.WFF.Application.Features.Solicitudes.Models.Flujo;

public sealed record SolicitudFirmanteFlujoRequest
(
    long Id,
    long IdUsuarioAsignado,
    string? CargoUsuario,
    string? AreaUsuario,
    string? GerenciaUsuario,
    long IdUsuarioOrigen
);
