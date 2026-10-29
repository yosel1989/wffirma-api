using COFIDE.WFF.Application.Common.Commands;

namespace COFIDE.WFF.Application.Features.AccesosEspeciales.Commands.CambiarEstadoAccesoEspecial
{
    public sealed record CambiarEstadoAccesoEspecialCommand
    (
        /// <summary>
        /// Identificador único del acceso especial.
        /// </summary>
        long Id,

        /// <summary>
        /// Nuevo estado que se le asignará el acceso especial, activo = true, inactivo = false.
        /// </summary>
        bool Estado

    ) : UpdateCommand<bool, long>(Id);
}
