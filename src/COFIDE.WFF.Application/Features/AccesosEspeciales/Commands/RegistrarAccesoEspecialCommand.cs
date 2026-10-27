using COFIDE.WFF.Application.Common.Commands;
namespace COFIDE.WFF.Application.Features.AccesosEspeciales.Commands
{
    public sealed record RegistrarAccesoEspecialCommand
    (
        /// <summary>
        /// Identificador único del usuario al que se le asignará el acceso especial.
        /// </summary>
        long IdUsuario,

        /// <summary>
        /// Identificador único del área (gerencia) asociada al acceso especial.
        /// </summary>
        long IdGerencia,

        /// <summary>
        /// Identificador único del área (departamento) asociado al acceso especial (opcional).
        /// </summary>
        long? IdDepartamento
    ) : CreateCommand<long?>;
}
