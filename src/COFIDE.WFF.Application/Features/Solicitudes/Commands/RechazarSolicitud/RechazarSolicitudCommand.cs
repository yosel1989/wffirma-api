using COFIDE.WFF.Application.Common.Commands;
using Microsoft.AspNetCore.Http;
namespace COFIDE.WFF.Application.Features.Solicitudes.Commands.RechazarSolicitud;

public sealed record RechazarSolicitudCommand
(
    long Id,
    string MotivoRechazo,
    string? Comentario,
    List<IFormFile>? Sustentos
) : UpdateCommand<string?, long>(Id);