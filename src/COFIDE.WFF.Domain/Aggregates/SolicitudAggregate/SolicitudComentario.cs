using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Errors;
using COFIDE.WFF.Domain.Common.Entities;
using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Domain.Exceptions;
using COFIDE.WFF.Domain.Common.Tiempo;
namespace COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
public class SolicitudComentario : AuditableEntity<long>
{
    public long IdSolicitud { get; private set; }
    public long IdUsuario { get; private set; }
    public string Comentario { get; private set; } = null!;
    public DateTime FechaComentario { get; private set; }
    public Usuario Usuario { get; private set; } = null!;
    public Solicitud Solicitud { get; private set; } = null!;
    public bool EsNuevo { get; private set; } = false;

    private SolicitudComentario() { } // EF

    private SolicitudComentario(
        long idSolicitud,
        long idSolicitudComentario,
        long idUsuario,
        string comentario)
    {
        IdSolicitud = idSolicitud;
        Id = idSolicitudComentario;
        IdUsuario = idUsuario;
        Comentario = comentario;
        FechaComentario = HoraSistema.Ahora;
        Estado = true;
    }

    public static SolicitudComentario Crear(
        long idSolicitud,
        long idSolicitudComentario,
        long idUsuario,
        string comentario)
    {
        if (idSolicitud <= 0)
            throw new DomainException(SolicitudErrors.SolicitudInvalida);

        if (string.IsNullOrWhiteSpace(comentario))
            throw new DomainException(SolicitudErrors.ComentarioRequired);

        var solicitudComentarioRpt = new SolicitudComentario
          (
              idSolicitud, idSolicitudComentario, idUsuario, comentario
          );

        solicitudComentarioRpt.MarcarComoNuevo();

        return solicitudComentarioRpt;
    }

    public void MarcarComoNuevo()
    {
        EsNuevo = true;
    }
}
