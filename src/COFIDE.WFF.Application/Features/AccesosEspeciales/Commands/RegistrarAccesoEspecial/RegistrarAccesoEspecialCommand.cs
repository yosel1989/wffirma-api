using COFIDE.WFF.Application.Common.Commands;
namespace COFIDE.WFF.Application.Features.AccesosEspeciales.Commands.RegistrarAccesoEspecial
{
    /// <summary>
    /// Comando para registrar un acceso especial, que permite a un usuario
    /// visualizar las solicitudes de otra gerencia y/o departamento.
    /// </summary>
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
