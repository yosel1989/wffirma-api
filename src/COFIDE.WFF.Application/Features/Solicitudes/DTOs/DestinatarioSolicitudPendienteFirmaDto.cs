namespace COFIDE.WFF.Application.Features.Solicitudes.DTOs;

/// <summary>Destinatario en turno y su total de solicitudes pendientes, vistas o no.</summary>
/// <param name="CodigoUsuario">Cuenta de logueo del destinatario en el hub.</param>
/// <param name="TotalSolicitudesPendientesFirma">Cantidad de solicitudes distintas pendientes en su turno.</param>
public sealed record DestinatarioSolicitudPendienteFirmaDto(string CodigoUsuario, int TotalSolicitudesPendientesFirma);
