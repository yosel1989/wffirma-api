using COFIDE.WFF.Application.Common.Files;
namespace COFIDE.WFF.Application.Features.Solicitudes.DTOs;

public sealed record SolicitudRequestDto(
    long IdTipoSolicitud = 0,
    long IdSubTipoSolicitud = 0,
    long IdtipoOperacion = 0,
    string? Asunto = null,
    string? Mensaje = null,
    bool EsUrgente = false,
    bool InCodigo = false,
    DateTime? FechaLimite = null,
    decimal? PesoTotalDocu = null,
    List<ArchivoEntrada>? Documentos = null,
    List<ArchivoEntrada>? Sustentos = null,
    List<long>? IdsDocumentosConservar = null,
    List<long>? IdsSustentosConservar = null,
    List<string>? CodigosReferencia = null,
    long? IdPlantilla = null,
    string? Configuracion = null,
    List<string>? Correos = null,
    string? Tags = null
);