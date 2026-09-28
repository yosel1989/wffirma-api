using COFIDE.WFF.Application.Common.Commands;

namespace COFIDE.WFF.Application.Features.Jobs.ActualizarEstadoProgramado
{
    public sealed record ActualizarEstadosProgramadoCommand(long Id)
    : UpdateCommand<int, long>(Id);
}
