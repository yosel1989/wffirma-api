using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Vo;
using COFIDE.WFF.Domain.Common.Entities;
namespace COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
public class SolicitudCodReferencia : AuditableEntity<long>
{
    public long IdSolicitud { get; private set; }
    public CodigoReferencia CodigoReferencia { get; private set; } = default!;
    public Solicitud Solicitud { get; set; } = default!;

    public bool EsNuevo { get; private set; } = false;
    private SolicitudCodReferencia() { }

    private SolicitudCodReferencia(
            long idSolicitud,
            long idSolicitudCodReferencia,
            CodigoReferencia codigoReferencia)
    {
        Id = idSolicitudCodReferencia;
        IdSolicitud = idSolicitud;
        CodigoReferencia = codigoReferencia;
        Estado = true;
    }

    public static SolicitudCodReferencia Crear(
    long idSolicitud,
    long idSolicitudCodReferencia,
    string codigoReferencia)
    {
        var codigoReferenciaVO = CodigoReferencia.Crear(codigoReferencia);

        var solicitudCodigoReferencia = new SolicitudCodReferencia
        (
           idSolicitud, idSolicitudCodReferencia, codigoReferenciaVO
        );

        solicitudCodigoReferencia.MarcarComoNuevo();

        return solicitudCodigoReferencia;
    }

    public static SolicitudCodReferencia Crear(
    long idSolicitud,
    long idSolicitudCodReferencia,
    CodigoReferencia codigoReferencia)
    {
        return new SolicitudCodReferencia
        (
           idSolicitud, idSolicitudCodReferencia, codigoReferencia
        );
    }

    public void Inactivar()
    {
        Estado = false;
    }

    public void MarcarComoNuevo()
    {
        EsNuevo = true;
    }
}