using COFIDE.WFF.Domain.Common.Entities;

namespace COFIDE.WFF.Domain.Entities
{
    public class ParametroCabecera : AuditableEntity<long>
    {
        public string CodigoParametro { get; set; } = null!;
        public string DescripcionParametro { get; set; } = null!;
        public ICollection<ParametroDetalle> Detalles { get; set; } = new List<ParametroDetalle>();
    }
}
