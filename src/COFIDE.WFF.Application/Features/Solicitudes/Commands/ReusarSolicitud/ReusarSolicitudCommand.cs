using COFIDE.WFF.Application.Common.Commands;
using COFIDE.WFF.Application.Features.Solicitudes.DTOs;
namespace COFIDE.WFF.Application.Features.Solicitudes.Commands.ReusarSolicitud;
public sealed record ReusarSolicitudCommand(
    long IdSolicitudOrigen,
    SolicitudRequestDto Request
) : CreateCommand<string>;