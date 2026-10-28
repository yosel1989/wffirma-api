using COFIDE.WFF.Application.Common.Commands;
namespace COFIDE.WFF.Application.Features.AccesosEspeciales.Commands.RegistrarAccesoEspecial
{
    public sealed record RegistrarAccesoEspecialCommand
    (
        /// <summary>
        /// Identificador único del usuario al que se le asignará el acceso especial.
        /// </summary>
        long IdUsuario,

        /// <summary>
        /// Identificador único del área (gerencia y/o departamento) asociada al acceso especial.
        /// </summary>
        long IdArea

    ) : CreateCommand<long>;
}
