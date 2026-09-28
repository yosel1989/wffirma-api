using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Errors;
using COFIDE.WFF.Domain.Common.Entities;
using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Domain.Exceptions;
namespace COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;

public class SolicitudEvento : AuditableEntity<long>
{
    public long IdSolicitud { get; private set; }
    public long IdTipoEvento { get; private set; }
    public long IdUsuarioOrigen { get; private set; }
    public long? IdUsuarioDestino { get; private set; }
    public long? IdPlantillaEtapa { get; private set; }
    public string? AreaUsuario { get; private set; }
    public long? IdSolicitudSustento { get; private set; }   
    public DateTime? FechaEvento { get; private set; }
    public Solicitud Solicitud { get; set; } = null!;
    public TipoEvento TipoEvento { get; set; } = null!;
    public Usuario UsuarioOrigen { get; set; } = null!;
    public Usuario? UsuarioDestino { get; set; }
    public PlantillaEtapa? PlantillaEtapa { get; set; }
    public SolicitudSustento? SolicitudSustento { get; set; }
    public bool EsNuevo { get; private set; } = false;

    private SolicitudEvento() { } // EF

    private SolicitudEvento(
        long idSolicitud,
        long idMovimientoSolicitud,
        long idTipoEvento,
        long idUsuarioOrigen,
        long? idUsuarioDestino,
        long? idPlantillaEtapa,
        string? areaUsuario,
        long? idSolicitudSustento,        
        DateTime fechaEvento)
    {
        IdSolicitud = idSolicitud;
        Id = idMovimientoSolicitud;
        IdTipoEvento = idTipoEvento;
        IdUsuarioOrigen = idUsuarioOrigen;
        IdUsuarioDestino = idUsuarioDestino;
        IdPlantillaEtapa = idPlantillaEtapa;
        AreaUsuario = areaUsuario;
        IdSolicitudSustento = idSolicitudSustento;        
        FechaEvento = fechaEvento;
        Estado = true;
    }

    public static SolicitudEvento Crear(
        long idSolicitud,
        long idMovimientoSolicitud,
        long idTipoEvento,
        long idUsuarioOrigen,
        long? idUsuarioDestino,
        long? idPlantillaEtapa,
        string? areaUsuario,
        long? idSolicitudSustento,
        DateTime fechaEvento)
    {
        if (idSolicitud <= 0)
            throw new DomainException(SolicitudErrors.SolicitudInvalida);

        if (idTipoEvento <= 0)
            throw new DomainException(SolicitudErrors.TipoEventoInvalido);

        if (idUsuarioOrigen <= 0)
            throw new DomainException(SolicitudErrors.UsuarioInvalido);

        var solicitudEventoRpt =  new SolicitudEvento
        (
            idSolicitud, idMovimientoSolicitud, idTipoEvento, idUsuarioOrigen, idUsuarioDestino, idPlantillaEtapa, areaUsuario, idSolicitudSustento, fechaEvento
        );

        solicitudEventoRpt.MarcarComoNuevo();

        return solicitudEventoRpt;
    }

    public void MarcarComoNuevo()
    {
        EsNuevo = true;
    }
}
