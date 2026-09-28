namespace COFIDE.WFF.Domain.ValueObjects;
public record FormatoNotificacionSolicitud(
 string TipoSolicitud,
 string CodigoSolicitud,
 string correoSolicitante,
 string NombreSolicitante,
 string SubTipoSolicitud,
 string TipoOperacion,
 string AsuntoSolicitud,
 string FechaCreacion,
 bool EsUrgente,
 DateTime? FechaLimite,
 string? MotivoRechazo,
 long? IdSolicitudPlantilla,
 bool TieneDocumento);