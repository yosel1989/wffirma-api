using COFIDE.WFF.Application.Common.Commands;
using Microsoft.AspNetCore.Http;
namespace COFIDE.WFF.Application.Features.Solicitudes.Commands.RegistrarSolicitud;

public sealed record RegistrarSolicitudCommand
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
    List<string>? CodigosReferencia,
    long? IdPlantilla,
    string? Configuracion,
    List<string>? Correos,
    string? Tags
) : CreateCommand<string?>;

