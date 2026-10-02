using COFIDE.WFF.Application.Common.Commands;
using COFIDE.WFF.Application.Common.Files;
namespace COFIDE.WFF.Application.Features.Solicitudes.Commands.AprobarSolicitud;

public sealed record AprobarSolicitudCommand
(
    long Id,
    string? Comentario,
    List<ArchivoEntrada>? Sustentos
) : UpdateCommand<string?, long>(Id);