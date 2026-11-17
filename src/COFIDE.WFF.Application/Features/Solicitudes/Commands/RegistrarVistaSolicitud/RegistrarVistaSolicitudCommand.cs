using COFIDE.WFF.Application.Common.Commands;

namespace COFIDE.WFF.Application.Features.Solicitudes.Commands.RegistrarVistaSolicitud;

public sealed record RegistrarVistaSolicitudCommand(long Id) : UpdateCommand<DateTime, long>(Id);
