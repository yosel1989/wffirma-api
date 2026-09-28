using Microsoft.AspNetCore.Http;
namespace COFIDE.WFF.Application.Features.Solicitudes.Models;
public sealed record EditarEnviarSolicitudRequest
(
    long IdTipoSolicitud,
    long IdSubTipoSolicitud,
    long IdtipoOperacion,
    string Asunto,
    string? Mensaje,
    bool EsUrgente,
    bool InCodigo,
    DateTime? FechaLimite,
    decimal? PesoTotalDocu,
    List<IFormFile>? Documentos,
    List<IFormFile>? Sustentos,
    List<long>? IdsDocumentosConservar,
    List<long>? IdsSustentosConservar,
    List<string>? CodigosReferencia,
    List<string>? Correos,
    long? IdPlantilla,
    string? Configuracion
);