namespace COFIDE.WFF.Application.Features.Solicitudes.Models.Flujo;

public sealed record SolicitudEtapaRequest
(
    long Id,
    Guid TempId,
    long OrdenEtapa,
    string NombreEtapa,
    long IdTipoEtapa,
    long IdTipoAprobador,
    List<SolicitudFirmanteFlujoRequest>? Firmantes
);