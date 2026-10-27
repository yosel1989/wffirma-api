using COFIDE.WFF.Domain.Aggregates.AccesoEspecialAggregate.Inputs;

namespace COFIDE.WFF.Domain.Aggregates.AccesoEspecialAggregate.Factories
{
    /// <summary>
    /// Proporciona métodos de fábrica para crear instancias de <see cref="AccesoEspecial"/>.
    /// </summary>
    public static class AccesoEspecialFactory
    {
        /// <summary>
        /// Crea una nueva instancia de <see cref="AccesoEspecial"/> utilizando los datos proporcionados en el input.
        /// </summary>
        /// <param name="input">Datos necesarios para registrar un acceso especial.</param>
        /// <returns>Una nueva instancia de <see cref="AccesoEspecial"/>.</returns>
        public static AccesoEspecial AccesoEspecialRegistrar(
            AccesoEspecialRegistrarInput input)
        {
            // Crear una nueva instancia de AccesoEspecial con los datos proporcionados.
            var accesoEspecial = AccesoEspecial.Crear(
                0,
                input.IdUsuario,
                input.IdArea
            );

            return accesoEspecial;
        }
    }
}
