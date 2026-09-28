using COFIDE.WFF.Application.Common.Commands;
using Microsoft.AspNetCore.Http;
namespace COFIDE.WFF.Application.Features.Solicitudes.Commands.AprobarSolicitud;

public sealed record AprobarSolicitudCommand
(
    long Id,
    string? Comentario,
    List<IFormFile>? Sustentos
) : UpdateCommand<string?, long>(Id);