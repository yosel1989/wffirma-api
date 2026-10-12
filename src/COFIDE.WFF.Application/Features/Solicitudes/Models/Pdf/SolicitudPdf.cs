namespace COFIDE.WFF.Application.Features.Solicitudes.Models.Pdf;

/// <summary>Datos de una solicitud para generar su formato en PDF.</summary>
public sealed record SolicitudPdf(
    string CodigoSolicitud,
    bool EsUrgente,
    string TipoSolicitud,
    string SubTipoSolicitud,
    string TipoOperacion,
    string Asunto,
    string? Mensaje,
    DateTime? FechaLimite,
    IReadOnlyList<FirmantePdf> Firmantes,
    IReadOnlyList<ArchivoPdf> ArchivosFirma,
    IReadOnlyList<ArchivoPdf> ArchivosSustento,
    IReadOnlyList<EtapaPdf> Etapas,
    IReadOnlyList<string> CodigosReferencia,
    IReadOnlyList<string> Destinatarios,
    DateTime FechaGeneracion);

/// <summary>Fila de la tabla Orden / Rol / Nombres.</summary>
public sealed record FirmantePdf(long Orden, string Rol, string Nombres);

/// <summary>Adjunto: solo nombre y tamaño (sin enlace de descarga).</summary>
public sealed record ArchivoPdf(string Nombre, long? PesoBytes);

/// <summary>Etapa del flujo de aprobaciones.</summary>
public sealed record EtapaPdf(long Orden, string TipoEtapa, string TipoAprobador, IReadOnlyList<AprobadorPdf> Aprobadores);

/// <summary>Aprobador de una etapa con su situación: Aprobado (con fecha), TuTurno o Pendiente.</summary>
public sealed record AprobadorPdf(string Nombre, EstadoAprobadorPdf Estado, string Detalle);

public enum EstadoAprobadorPdf
{
    Aprobado,
    TuTurno,
    Pendiente
}
