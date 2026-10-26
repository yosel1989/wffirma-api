namespace COFIDE.WFF.Api.Request.Solicitudes;

public sealed record DesencriptarIdSolicitudRequest(
    string Token,
    bool Revisar = true
);