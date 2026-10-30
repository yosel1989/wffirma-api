using COFIDE.WFF.Application.Common.Commands;

namespace COFIDE.WFF.Application.Features.Solicitudes.Commands.CambiarEstadoSolicitud;

public sealed record CambiarEstadoSolicitudCommand(
    long Id,
    long IdNuevoEstado,
    string Motivo)
    : UpdateCommand<string?, long>(Id);
