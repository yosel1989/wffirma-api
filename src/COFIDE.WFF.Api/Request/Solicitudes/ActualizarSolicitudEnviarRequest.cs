using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace COFIDE.WFF.Api.Request.Solicitudes;

public sealed record ActualizarSolicitudEnviarRequest
(
    long? IdTipoSolicitud,
    long? IdSubTipoSolicitud,
    long? IdtipoOperacion,
    string? Asunto,
    string? Mensaje,
    bool? EsUrgente,
    bool? InCodigo,
    DateTime? FechaLimite,
    decimal? PesoTotalDocu,
    List<IFormFile>? Documentos,
    List<IFormFile>? Sustentos,
    List<long>? IdsDocumentosConservar,
    List<long>? IdsSustentosConservar,
    List<string>? CodigosReferencia,
    List<string>? Correos,
    long? IdPlantilla,
    string? EtapasJson
);

