using COFIDE.WFF.Application.Features.Solicitudes.Models.Flujo;
namespace COFIDE.WFF.Api.Requests.Solicitudes;

public sealed record ActualizarSolicitudRequest
(
    long? IdTipoSolicitud,
    long? IdSubTipoSolicitud,
    long? IdtipoOperacion,
    string Asunto,
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
