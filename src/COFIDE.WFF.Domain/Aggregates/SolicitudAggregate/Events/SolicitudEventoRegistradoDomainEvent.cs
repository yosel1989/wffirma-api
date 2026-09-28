using COFIDE.WFF.Domain.Common.Abstractions;
using COFIDE.WFF.Domain.Common.Events;
namespace COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Events;

public sealed record SolicitudEventoRegistradoDomainEvent(
 long IdSolicitud,
 long IdTipoEvento,
 long IdUsuarioOrigen,
 long? IdUsuarioDestino,
 long? IdPlantillaEtapa,
 string? AreaUsuario,
 long? IdSolicitudSustento 
) : DomainEvent, IPostCommitDomainEvent;
