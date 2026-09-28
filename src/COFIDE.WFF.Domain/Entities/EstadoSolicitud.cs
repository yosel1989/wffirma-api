using COFIDE.WFF.Domain.Common.Entities;
namespace COFIDE.WFF.Domain.Entities;

    public class EstadoSolicitud : AuditableEntity<long>
    {
        public string? NombreEstadoSolicitud { get;}
        public string? Descripcion { get; }
    }

