using COFIDE.WFF.Application.Common.Commands;
using COFIDE.WFF.Application.Common.Files;
namespace COFIDE.WFF.Application.Features.Solicitudes.Commands.EnviarSolicitud;

public sealed record EnviarSolicitudCommand
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
    List<ArchivoEntrada>? Documentos,
    List<ArchivoEntrada>? Sustentos,
    List<string>? CodigosReferencia,
    long? IdPlantilla,
    string? Configuracion,
    List<string>? Correos,
    string? Tags
) : CreateCommand<string?>;

