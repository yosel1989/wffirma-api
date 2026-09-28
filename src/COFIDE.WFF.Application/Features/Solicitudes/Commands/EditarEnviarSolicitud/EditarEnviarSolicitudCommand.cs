using COFIDE.WFF.Application.Common.Commands;
using COFIDE.WFF.Application.Features.Solicitudes.DTOs;
namespace COFIDE.WFF.Application.Features.Solicitudes.Commands.EditarEnviarSolicitud;
public sealed record EditarEnviarSolicitudCommand
(
    long IdSolicitud,
    SolicitudRequestDto Request
) : CreateCommand<string>;