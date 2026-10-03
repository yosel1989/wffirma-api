using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Events;
using COFIDE.WFF.Domain.DomainServices;
using ValueObjects = COFIDE.WFF.Domain.ValueObjects;
namespace COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Factories;

/// <summary>
/// Crea los eventos de historial de una solicitud. Es lógica de dominio (qué tipo de evento
/// corresponde a cada cambio); el usuario y el área se reciben como datos, sin depender de la sesión.
/// </summary>
public static class SolicitudEventoFactory
{
    public static List<SolicitudEventoRegistradoDomainEvent> CrearEventosCambioFlujo(
        long idSolicitud,
        SolicitudFlujoDiff diff,
        long idUsuario,
        string areaUsuario)
    {
        var eventos = new List<SolicitudEventoRegistradoDomainEvent>();

        eventos.AddRange(diff.EtapasAgregadas.Select(etapa => Evento(
            idSolicitud, ValueObjects.TipoEvento.FlowStageAdded.Codigo, idUsuario, null, etapa.IdEtapa, areaUsuario, null)));

        eventos.AddRange(diff.EtapasEliminadas.Select(etapa => Evento(
            idSolicitud, ValueObjects.TipoEvento.FlowStageRemoved.Codigo, idUsuario, null, etapa.IdEtapa, areaUsuario, null)));

        eventos.AddRange(diff.FirmantesAgregados.Select(f => Evento(
            idSolicitud, ValueObjects.TipoEvento.FlowApproverAdded.Codigo, idUsuario, f.IdUsuario, f.IdEtapa, areaUsuario, null)));

        eventos.AddRange(diff.FirmantesEliminados.Select(f => Evento(
            idSolicitud, ValueObjects.TipoEvento.FlowApproverRemoved.Codigo, idUsuario, f.IdUsuario, f.IdEtapa, areaUsuario, null)));

        return eventos;
    }

    public static List<SolicitudEventoRegistradoDomainEvent> CrearEventoSustento(
        long idSolicitud,
        long idSustento,
        long idUsuario,
        string areaUsuario) =>
        [Evento(idSolicitud, ValueObjects.TipoEvento.DocumentAdded.Codigo, idUsuario, null, null, areaUsuario, idSustento)];

    private static SolicitudEventoRegistradoDomainEvent Evento(
        long idSolicitud,
        long idTipoEvento,
        long idUsuarioOrigen,
        long? idUsuarioDestino,
        long? idPlantillaEtapa,
        string areaUsuario,
        long? idSustento) =>
        new(idSolicitud, idTipoEvento, idUsuarioOrigen, idUsuarioDestino, idPlantillaEtapa, areaUsuario, idSustento);
}
