using COFIDE.WFF.Application.Common.Commands;
using COFIDE.WFF.Application.Common.Files;
namespace COFIDE.WFF.Application.Features.Solicitudes.Commands.RechazarSolicitud;

public sealed record RechazarSolicitudCommand
(
    long Id,
    string MotivoRechazo,
    string? Comentario,
    List<ArchivoEntrada>? Sustentos
) : UpdateCommand<string?, long>(Id);