using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Events;
using ValueObjects = COFIDE.WFF.Domain.ValueObjects;
namespace COFIDE.WFF.Application.Features.Solicitudes.Services;

public class SolicitudEventoFactory : ISolicitudEventoFactory
{
    private readonly ICurrentUserService _currentUser;
    public SolicitudEventoFactory(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    public List<SolicitudEventoRegistradoDomainEvent> CrearEventos(
        long idSolicitud,
        SolicitudFlujoDiff diff)//areaUsuario antes
    {

        long usuarioActual = _currentUser.UserIdLocal!.Value;
        string nombreGerencia = _currentUser.UserGerenciaNombre!;

        var eventos = new List<SolicitudEventoRegistradoDomainEvent>();

        foreach (var etapa in diff.EtapasAgregadas)
        {
            eventos.Add(new SolicitudEventoRegistradoDomainEvent(
                idSolicitud,
                ValueObjects.TipoEvento.FlowStageAdded.Codigo,
                usuarioActual,
                null,
                etapa.IdEtapa,
                nombreGerencia,//areaUsuario
                null
            ));
        }

        foreach (var etapa in diff.EtapasEliminadas)
        {
            eventos.Add(new SolicitudEventoRegistradoDomainEvent(
                idSolicitud,
                ValueObjects.TipoEvento.FlowStageRemoved.Codigo,
                usuarioActual,
                null,
                etapa.IdEtapa,
                nombreGerencia,//areaUsuario
                null
            ));
        }

       foreach (var f in diff.FirmantesAgregados)
        {
            eventos.Add(new SolicitudEventoRegistradoDomainEvent(
                idSolicitud,
                ValueObjects.TipoEvento.FlowApproverAdded.Codigo,
                usuarioActual,
                f.IdUsuario,
                f.IdEtapa,
                nombreGerencia,//areaUsuario
                null
            ));
        }

        foreach (var f in diff.FirmantesEliminados)
        {
            eventos.Add(new SolicitudEventoRegistradoDomainEvent(
                idSolicitud,
                ValueObjects.TipoEvento.FlowApproverRemoved.Codigo,
                usuarioActual,
                f.IdUsuario,
                f.IdEtapa,
                nombreGerencia,//areaUsuario
                null
            ));
        }
        return eventos;
    }


    public List<SolicitudEventoRegistradoDomainEvent> CrearEventoSustento(
    long idSolicitud,
    long idSustento)//areaUsuario antes
    {
        long usuarioActual = _currentUser.UserIdLocal!.Value;
        string nombreGerencia = _currentUser.UserGerenciaNombre!;

        var eventos = new List<SolicitudEventoRegistradoDomainEvent>();
            eventos.Add(new SolicitudEventoRegistradoDomainEvent(
                idSolicitud,
                ValueObjects.TipoEvento.DocumentAdded.Codigo,
                usuarioActual,
                null,
                null,
                nombreGerencia,//areaUsuario
                idSustento
            ));

        return eventos;
    }
}
