using COFIDE.WFF.Application.Common.Commands;
using COFIDE.WFF.Application.Features.Solicitudes.DTOs;
namespace COFIDE.WFF.Application.Features.Solicitudes.Commands.EditarSolicitud;

public sealed record EditarSolicitudCommand(
    long IdSolicitud,
    SolicitudRequestDto Request
) : CreateCommand<string>;