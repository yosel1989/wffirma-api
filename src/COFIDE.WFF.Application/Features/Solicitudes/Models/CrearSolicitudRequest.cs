using Microsoft.AspNetCore.Http;
namespace COFIDE.WFF.Application.Features.Solicitudes.Models;

public sealed record CrearSolicitudRequest(
    long IdTipoSolicitud,
    long IdSubTipoSolicitud,
    long IdtipoOperacion,
    string? Asunto,
    string? Mensaje,
    bool EsUrgente,
    bool InCodigo,
    DateTime? FechaLimite,
    decimal? PesoTotalDocu,
    List<IFormFile>? Documentos,
    List<IFormFile>? Sustentos,
    List<string>? CodigosReferencia,
    long? IdPlantilla,
    string? Configuracion,
    List<string>? Correos
);
