using COFIDE.WFF.Application.Common.Commands;
using COFIDE.WFF.Application.Features.Solicitudes.DTOs;
namespace COFIDE.WFF.Application.Features.Solicitudes.Commands.ReusarEnviarSolicitud;
public sealed record ReusarEnviarSolicitudCommand(
    long IdSolicitudOrigen,
    SolicitudRequestDto Request
) : CreateCommand<string>;