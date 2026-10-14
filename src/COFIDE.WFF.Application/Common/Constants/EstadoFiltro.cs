namespace COFIDE.WFF.Application.Common.Constants
{
    /// <summary>
    /// Códigos permitidos para filtrar registros por estado en las consultas.
    /// </summary>
    public static class EstadoFiltro
    {
        /// <summary>
        /// Solo registros activos ('A'). Es el valor por defecto de las consultas.
        /// </summary>
        public const char Activo = 'A';

        /// <summary>
        /// Solo registros inactivos ('I').
        /// </summary>
        public const char Inactivo = 'I';

        /// <summary>
        /// Todos los registros, sin filtrar por estado ('T').
        /// </summary>
        public const char Todos = 'T';
    }
}
