using COFIDE.WFF.Application.Common.Commands;

namespace COFIDE.WFF.Application.Features.Solicitudes.Commands.RestaurarSolicitud;

public sealed record RestaurarSolicitudCommand(long Id, long IdPuntoRestauracion)
    : UpdateCommand<string?, long>(Id);
