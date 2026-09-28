using COFIDE.WFF.Application.Common.Commands;
namespace COFIDE.WFF.Application.Features.Solicitudes.Commands.AnularSolicitud;
public sealed record AnularSolicitudCommand(long IdSolicitud) : CreateCommand<string>;

