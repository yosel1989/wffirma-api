namespace COFIDE.WFF.Domain.Aggregates.AccesoEspecialAggregate.Inputs
{
    /// <summary>  
    /// Representa los datos necesarios para registrar un acceso especial.  
    /// </summary>  
    public class AccesoEspecialRegistrarInput
    {
        /// <summary>  
        /// Identificador único del usuario.  
        /// </summary>  
        public long IdUsuario { get; init; }

        /// <summary>  
        /// Identificador único del área (gerencia y/o departamento).  
        /// </summary>  
        public long IdArea { get; init; }
    }
}
