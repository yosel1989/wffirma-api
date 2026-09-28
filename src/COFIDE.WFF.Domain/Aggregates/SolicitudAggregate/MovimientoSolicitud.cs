using COFIDE.WFF.Domain.Common.Entities;
using COFIDE.WFF.Domain.Entities;
using ValueObject = COFIDE.WFF.Domain.ValueObjects;
namespace COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;

public class MovimientoSolicitud : AuditableEntity<long>
{
    public long IdSolicitud { get; private set; }
    public long IdEstadoSolicitud { get; private set; }
    public long IdUsuarioEstado { get; private set; }
    public DateTime FechaEstado { get; private set; }
    public ValueObject.MotivoCambio? MotivoCambio { get; private set; }
    public Usuario UsuarioEstado { get; set; } = null!;
    public bool EsNuevo { get; private set; } = false;
    private MovimientoSolicitud() { }
    private MovimientoSolicitud(
        long idSolicitud,
        long idMovimientoSolicitud,
        long idEstadoSolicitud,
        long idUsuarioEstado,
        DateTime fechaEstado,
        ValueObject.MotivoCambio? motivoCambio)
    {        
        IdSolicitud = idSolicitud;
        Id = idMovimientoSolicitud;
        IdEstadoSolicitud = idEstadoSolicitud;
        IdUsuarioEstado = idUsuarioEstado;
        FechaEstado = fechaEstado;
        MotivoCambio = motivoCambio;
        Estado = true;
    }
    public static MovimientoSolicitud Crear(
        long idSolicitud,
        long idMovimientoSolicitud,
        long idEstadoSolicitud,
        long idUsuarioEstado,
        DateTime fechaEstado,
        string? motivoCambio)
    {
        var motivoCambioVO = ValueObject.MotivoCambio.Crear(motivoCambio);

        var movimientoSolicitudRpt =  new MovimientoSolicitud
        (
            idSolicitud, idMovimientoSolicitud, idEstadoSolicitud, idUsuarioEstado, fechaEstado, motivoCambioVO
        );

        movimientoSolicitudRpt.MarcarComoNuevo();

        return movimientoSolicitudRpt;
    }

    public void MarcarComoNuevo()
    {
        EsNuevo = true;
    }
}
