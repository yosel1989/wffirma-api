using COFIDE.WFF.Domain.Common.Entities;

namespace COFIDE.WFF.Domain.Entities
{
    public class ParametroCabecera : AuditableEntity<long>
    {
        public string CodigoParametro { get; private set; } = null!;
        public string DescripcionParametro { get; private set; } = null!;
        public ICollection<ParametroDetalle> Detalles { get; private set; } = new List<ParametroDetalle>();
    }
}
