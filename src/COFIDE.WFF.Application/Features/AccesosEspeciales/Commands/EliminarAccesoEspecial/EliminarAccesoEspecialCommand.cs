using COFIDE.WFF.Application.Common.Commands;

namespace COFIDE.WFF.Application.Features.AccesosEspeciales.Commands.EliminarAccesoEspecial
{
    /// <summary>
    /// Comando para eliminar físicamente un acceso especial.
    /// </summary>
    /// <param name="Id">Identificador único del acceso especial.</param>
    public sealed record EliminarAccesoEspecialCommand(long Id)
        : DeleteCommand<bool, long>(Id);
}